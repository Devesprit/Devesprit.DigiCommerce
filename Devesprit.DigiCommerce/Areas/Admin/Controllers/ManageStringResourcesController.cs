using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageStringResourcesController : BaseController
    {
        private readonly ILocalizationService _localizationService;

        public ManageStringResourcesController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public virtual async Task<ActionResult> Index(int langId)
        {
            var language = await LanguagesService.FindByIdAsync(langId);

            if (language == null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }
            
            ViewBag.LangId = langId;
            ViewBag.LangName = language.LanguageName;
            ViewBag.LanguagesDropDown = LanguagesService.GetAsEnumerable().Select(p => new
            {
                text = p.LanguageName,
                value = p.Id.ToString()
            }).ToList();


            return View();
        }

        public virtual async Task<ActionResult> ExportResources(int langId)
        {
            var language = await LanguagesService.FindByIdAsync(langId);
            if (language == null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            var xmlString = await _localizationService.ExportResourcesToXmlAsync(language);

            var contentType = "text/xml";
            var bytes = Encoding.UTF8.GetBytes(xmlString);
            var result = new FileContentResult(bytes, contentType);
            result.FileDownloadName = $"DigiCommerce.StringResources.{language.IsoCode}.xml";

            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ImportResources(int langId, HttpPostedFileBase xmlFile)
        {
            var language = await LanguagesService.FindByIdAsync(langId);
            if (language == null)
            {
                return RedirectToAction("PageNotFound", "Error");
            }

            try
            { 
                string xmlContent = new StreamReader(xmlFile.InputStream).ReadToEnd();
                await _localizationService.ImportResourcesFromXmlAsync(language.Id, xmlContent);
                SuccessNotification(_localizationService.GetResource("StringResourcesImportedSuccessfully"));
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ErrorNotification(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message,
                    errorCode));
            }

            return RedirectToAction("Index", new { langId = langId});
        }

        [HttpPost]
        public virtual ActionResult Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    _localizationService.Delete(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult Update(TblLocalizedStrings value)
        {
            _localizationService.Update(value);
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Insert(TblLocalizedStrings value)
        {
            _localizationService.Add(value);
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GridDataSource(DataManager dm, int filterByLangId)
        {
            var query = _localizationService.GetAsQueryable().Where(p => p.LanguageId == filterByLangId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.ResourceName,
                p.ResourceValue,
                p.LanguageId,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}