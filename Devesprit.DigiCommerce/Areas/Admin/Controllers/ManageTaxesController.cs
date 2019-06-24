using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Taxes;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageTaxesController : BaseController
    {
        private readonly ITaxesService _taxesService;
        private readonly ITaxModelFactory _taxModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManageTaxesController(ITaxesService taxesService,
            ITaxModelFactory taxModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _taxesService = taxesService;
            _taxModelFactory = taxModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _taxesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _taxModelFactory.PrepareTaxModelAsync(record));
                }
            }

            return View(await _taxModelFactory.PrepareTaxModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(TaxModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _taxModelFactory.PrepareTblTaxes(model);
            var recordId = model.Id;
            
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _taxesService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _taxesService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ManageTaxes", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshTaxesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _taxesService.DeleteAsync(key);
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
            var query = _taxesService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.TaxName,
                p.Amount,
                p.IsActive
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}