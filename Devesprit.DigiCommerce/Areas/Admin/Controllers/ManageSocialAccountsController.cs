using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.SocialAccounts;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageSocialAccountsController : BaseController
    {
        private readonly ISocialAccountsService _socialAccountsService;
        private readonly ISocialAccountModelFactory _socialAccountModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManageSocialAccountsController(ISocialAccountsService socialAccountsService,
            ISocialAccountModelFactory socialAccountModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _socialAccountsService = socialAccountsService;
            _socialAccountModelFactory = socialAccountModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult GridView()
        {
            return PartialView();
        }

        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _socialAccountsService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _socialAccountModelFactory.PrepareSocialAccountModelAsync(record));
                }
            }

            return View(await _socialAccountModelFactory.PrepareSocialAccountModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(SocialAccountModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _socialAccountModelFactory.PrepareTblSocialAccounts(model);
            var recordId = model.Id;

            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _socialAccountsService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _socialAccountsService.UpdateAsync(record);
                }

                await _localizedEntityService.SaveAllLocalizedStringsAsync(record, model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManageSocialAccounts", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshSocialAccountsGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _socialAccountsService.DeleteAsync(key);
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
            var query = _socialAccountsService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.SocialNetworkName,
                p.SocialNetworkIconUrl,
                p.YourAccountUrl
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}