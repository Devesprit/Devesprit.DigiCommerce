using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageCurrenciesController : BaseController
    {
        private readonly ICurrencyModelFactory _currencyModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManageCurrenciesController(ICurrencyModelFactory currencyModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _currencyModelFactory = currencyModelFactory;
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
                var record = await CurrencyService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _currencyModelFactory.PrepareCurrencyModelAsync(record));
                }
            }

            return View(await _currencyModelFactory.PrepareCurrencyModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(CurrencyModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _currencyModelFactory.PrepareTblCurrencies(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await CurrencyService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await CurrencyService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ManageCurrencies", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshCurrenciesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await CurrencyService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> SetCurrencyAsDefault(int id)
        {
            try
            {
                await CurrencyService.SetAsDefaultAsync(id);
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
            var query = CurrencyService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.CurrencyName,
                p.ShortName,
                p.DisplayFormat,
                p.IsoCode,
                p.IsMainCurrency,
                p.ExchangeRate,
                p.Published,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}