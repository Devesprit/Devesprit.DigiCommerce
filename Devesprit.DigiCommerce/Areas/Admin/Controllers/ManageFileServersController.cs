using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.FileServers;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageFileServersController : BaseController
    {
        private readonly IFileServersService _fileServersService;
        private readonly ILocalizationService _localizationService;
        private readonly IFileServerModelFactory _fileServerModelFactory;

        public ManageFileServersController(IFileServersService fileServersService,
            ILocalizationService localizationService,
            IFileServerModelFactory fileServerModelFactory)
        {
            _fileServersService = fileServersService;
            _localizationService = localizationService;
            _fileServerModelFactory = fileServerModelFactory;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _fileServersService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(_fileServerModelFactory.PrepareFileServerModel(record));
                }
            }

            return View(_fileServerModelFactory.PrepareFileServerModel(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(FileServerModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _fileServerModelFactory.PrepareTblFileServers(model);
            var recordId = model.Id;
            try
            {
                var server = _fileServersService.GetWebService(record);
                var result = await server.EnumerateFilesAsync("\\", "*.*", false, TimeSpan.Zero, 0).ConfigureAwait(false);
                server.Close();
            }
            catch(Exception ex)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(ex, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), ex.Message, errorCode));
                return View(model);
            }

            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _fileServersService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _fileServersService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ManageFileServers", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshFileServersGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _fileServersService.DeleteAsync(key);
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
            var query = _fileServersService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.FileServerName,
                p.FileServerUrl,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}