using System;
using System.Linq;
using System.Web.Mvc;
using CronExpressionDescriptor;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Hangfire;
using Hangfire.Storage;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageBackgroundJobsController : BaseController
    {
        private readonly IBackgroundJobModelFactory _backgroundJobModelFactory;
        private readonly ILocalizationService _localizationService;

        public ManageBackgroundJobsController(IBackgroundJobModelFactory backgroundJobModelFactory,
            ILocalizationService localizationService)
        {
            _backgroundJobModelFactory = backgroundJobModelFactory;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult Editor(string id)
        {
            if (id != null)
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var record = connection.GetRecurringJobs().FirstOrDefault(p => p.Id == id);
                    if (record != null)
                    {
                        var pausedJobs = connection.GetAllItemsFromSet("paused-jobs");
                        return View(_backgroundJobModelFactory.PrepareBackgroundJobModel(record, pausedJobs));
                    }
                }
            }

            return RedirectToAction("PageNotFound", "Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Editor(BackgroundJobModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var record = connection.GetRecurringJobs().FirstOrDefault(p => p.Id == model.Id);
                    if (record != null)
                    {
                        new RecurringJobManager(JobStorage.Current).AddOrUpdate(record.Id, record.Job, model.Cron,
                            TimeZoneInfo.FindSystemTimeZoneById(model.TimeZoneId));
                    }
                }
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManageBackgroundJobs", new { id = model.Id });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshBgJobsGrid();
                             </script>");
        }

        [HttpPost]
        public virtual ActionResult Delete(string[] keys)
        {
            try
            {
                foreach (var key in keys)
                    RecurringJob.RemoveIfExists(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual ActionResult PauseResumeJob(string jobId, bool pause)
        {
            try
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var record = connection.GetRecurringJobs().FirstOrDefault(p => p.Id == jobId);
                    if (record != null)
                    {
                        using (var transaction = connection.CreateWriteTransaction())
                        {
                            if (pause)
                            {
                                transaction.AddToSet("paused-jobs", record.Job.ToString());
                            }
                            else
                            {
                                transaction.RemoveFromSet("paused-jobs", record.Job.ToString());
                            }
                            
                            transaction.Commit();
                        }
                    }
                }
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }
        
        [HttpPost]
        public virtual ActionResult ExecuteJobNow(string jobId)
        {
            try
            {
                new RecurringJobManager(JobStorage.Current).Trigger(jobId);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var dataSource = connection.GetRecurringJobs();
                var pausedJobs = connection.GetAllItemsFromSet("paused-jobs");
                var urlTemplate = "<a href='/hangfire/jobs/details/{0}' target='_blank'>{1}</a>";
                var result = dataSource.ToList().Select(p => new
                {
                    p.Id,
                    Cron = ExpressionDescriptor.GetDescription(p.Cron) + $"<br/><small>{p.Cron}</small>",
                    LastExecution = p.LastExecution != null ? urlTemplate.FormatWith(p.LastJobId, p.LastExecution.Value.ToLocalTime().ToString("F")) : "-",
                    p.LastJobState,
                    NextExecution = p.NextExecution?.ToLocalTime().ToString("F") ?? "-",
                    p.TimeZoneId,
                    Job = p.Id,
                    Paused = pausedJobs.Contains(p.Id)
                });
                result = result.AsQueryable().ApplyDataManager(dm, out var count).ToList();
                return Json(dm.RequiresCounts ? new {result = result, count = count} : (object) result,
                    JsonRequestBehavior.AllowGet);
            }
        }
    }
}