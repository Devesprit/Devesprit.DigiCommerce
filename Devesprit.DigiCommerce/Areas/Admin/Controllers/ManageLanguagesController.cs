using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageLanguages")]
    public partial class ManageLanguagesController : BaseController
    {
        private readonly ILanguageModelFactory _languageModelFactory;
        private readonly ILocalizationService _localizationService;

        public ManageLanguagesController(ILanguageModelFactory languageModelFactory,
            ILocalizationService localizationService)
        {
            _languageModelFactory = languageModelFactory;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [UserHasAtLeastOnePermission("ManageLanguages_Add", "ManageLanguages_Edit")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await LanguagesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _languageModelFactory.PrepareLanguageModelAsync(record));
                }
            }

            return View(await _languageModelFactory.PrepareLanguageModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageLanguages_Add", "ManageLanguages_Edit")]
        public virtual async Task<ActionResult> Editor(LanguageModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                model.CurrenciesList = await CurrencyService.GetAsSelectListAsync();
                return View(model);
            }

            var record = _languageModelFactory.PrepareTblLanguages(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageLanguages_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await LanguagesService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageLanguages_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await LanguagesService.UpdateAsync(record);
                }
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                model.CurrenciesList = await CurrencyService.GetAsSelectListAsync();
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManageLanguages", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshLanguagesGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageLanguages_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await LanguagesService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        [UserHasPermission("ManageLanguages_SetLanguageAsDefault")]
        public virtual async Task<ActionResult> SetLanguageAsDefault(int id)
        {
            try
            {
                await LanguagesService.SetAsDefaultAsync(id);
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
            var query = LanguagesService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.LanguageName,
                p.IsoCode,
                p.IsDefault,
                p.IsRtl,
                p.Published,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}