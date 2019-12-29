using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.DigiCommerce.Models.Invoice;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Mapster;
using Plugin.DiscountCode.DB;
using Plugin.DiscountCode.Models;
using Syncfusion.JavaScript;
using Z.EntityFramework.Plus;

namespace Plugin.DiscountCode.Controllers
{
    public partial class DiscountCodeController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IInvoiceService _invoiceService;
        private readonly DiscountCodeDbContext _dbContext;

        public DiscountCodeController(ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IInvoiceService invoiceService,
            DiscountCodeDbContext dbContext)
        {
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _invoiceService = invoiceService;
            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual ActionResult Configure()
        {
            return View("~/Plugins/Plugin.DiscountCode/Views/Configure.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual ActionResult InvoiceList(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                var url = Url.Action("ConfigPlugin", "ManagePlugins", new RouteValueDictionary
                {
                    {"pluginName", "DiscountProcessor.DiscountCode"},
                    {"area", "Admin"}
                });
                return Redirect(url);
            }
            ViewBag.DiscountCode = code;
            return View("~/Plugins/Plugin.DiscountCode/Views/InvoiceList.cshtml");
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _dbContext.DiscountCode
                    .FirstOrDefaultAsync(p => p.Id == id);
                if (record != null)
                {
                    var model = record.Adapt<DiscountCodeViewModel>();
                    await record.LoadAllLocalizedStringsToModelAsync(model);
                    return View("~/Plugins/Plugin.DiscountCode/Views/Editor.cshtml", model);
                }
            }
            return View("~/Plugins/Plugin.DiscountCode/Views/Editor.cshtml", new DiscountCodeViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual async Task<ActionResult> Editor(DiscountCodeViewModel model, bool? saveAndContinue)
        {
            if (model.Id == null && await _dbContext.DiscountCode.AnyAsync(p => p.DiscountCode == model.DiscountCode))
            {
                ModelState.AddModelError("DiscountCode", _localizationService.GetResource("Plugin.DiscountCode.DiscountCodeDuplicateError"));
            }

            if (!ModelState.IsValid)
            {
                return View("~/Plugins/Plugin.DiscountCode/Views/Editor.cshtml", model);
            }

            var record = model.Adapt<TblDiscountCodes>();
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    _dbContext.DiscountCode.Add(record);
                    await _dbContext.SaveChangesAsync();
                    recordId = record.Id;
                }
                else
                {
                    //Edit record
                    _dbContext.DiscountCode.AddOrUpdate(record);
                    await _dbContext.SaveChangesAsync();
                }

                await _localizedEntityService.SaveAllLocalizedStringsAsync(record, model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View("~/Plugins/Plugin.DiscountCode/Views/Editor.cshtml", model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshDiscountCodesGrid();
                             </script>");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                {
                    await _dbContext.DiscountCode.Where(p => p.Id == key).DeleteAsync();
                    await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblDiscountCodes).Name, key);
                }
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var dataSource = _dbContext.DiscountCode;

            var result = dataSource.ApplyDataManager(dm, out var count).ToList().Select(p => new
            {
                p.Id,
                p.DiscountCode,
                p.DiscountCodeTitle,
                DiscountAmount = p.IsPercentage ? p.DiscountAmount.ToString("N2")+"%" : p.DiscountAmount.ToString("N2"),
                p.IsPercentage,
                ExpiryDate = p.ExpiryDate == null ? "-" : p.ExpiryDate.Value.ToString("G"),
                MaxNumberOfUsage = p.MaxNumberOfUsage == null ? "-" : p.MaxNumberOfUsage.Value.ToString("N0"),
                NumberOfUsed = _dbContext.InvoicesDiscountCode.Count(x=> x.DiscountCode == p.DiscountCode).ToString("N0")
            });
            return Json(new { result = result, count = count });
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission("DevespritDiscountCodeConfig")]
        public virtual ActionResult InvoiceGridDataSource(DataManager dm, string code)
        {
            var dataSource = _dbContext.InvoicesDiscountCode.Where(p=> p.DiscountCode == code).ApplyDataManager(dm, out var count).ToList();
            var invoiceIds = dataSource.Select(x => x.InvoiceId).ToList();
            var result = _invoiceService.GetAsQueryable()
                .Where(n => invoiceIds.Contains(n.Id)).ToList().Select(y =>
                    new
                    {
                        y.Id,
                        Status = y.Status.ToString(),
                        ItemsCount = y.InvoiceDetails.Count,
                        BillingAddress = y.BillingAddress?.FirstOrDefault()?.StreetAddress ?? "-",
                        CreateDate = y.CreateDate.ToString("d"),
                        PaymentDate = y.PaymentDate?.ToString("d") ?? "-",
                        PaymentGatewayTransactionId = y.PaymentGatewayTransactionId ?? "-",
                        PaymentGatewayName = y.PaymentGatewayName ?? "-",
                        UserName = y.User?.UserName ?? "-",
                        Currency = y.Currency?.CurrencyName ?? "-",
                        Total = y.ComputeInvoiceTotalAmount(false).ToString("N2"),
                        DiscountAmount = y.DiscountAmount?.ToString("N2") ?? "-"
                    });
            return Json(new { result = result, count = count });
        }

        [ChildActionOnly]
        public virtual ActionResult Index(string widgetZone, object additionalData = null)
        {
            if (additionalData is InvoiceModel invoiceModel)
            {
                var tblDiscountCode = _dbContext.InvoicesDiscountCode.FirstOrDefault(p => p.InvoiceId == invoiceModel.Id);
                var model = new ApplyDiscountCodeViewModel()
                {
                    InvoiceId = invoiceModel.Id,
                    InvoiceStatus = invoiceModel.Status,
                    DiscountCode = tblDiscountCode?.DiscountCode
                };
                return View("~/Plugins/Plugin.DiscountCode/Views/ApplyDiscountCode.cshtml", model);
            }

            return Content("");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> ApplyDiscountCode(ApplyDiscountCodeViewModel model)
        {
            var tblInvoice = await _invoiceService.FindByIdAsync(model.InvoiceId);
            if (tblInvoice == null)
            {
                return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
            }
            if (string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                await _dbContext.InvoicesDiscountCode.Where(p => p.InvoiceId == model.InvoiceId).DeleteAsync();
                await _invoiceService.ApplyInvoiceDiscountsAsync(tblInvoice.Id);
                return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
            }

            model.DiscountCode = model.DiscountCode.Trim();
            var tblDiscountCode = await _dbContext.DiscountCode.FirstOrDefaultAsync(p=> p.DiscountCode == model.DiscountCode);
            if (tblDiscountCode == null)
            {
                TempData["discountCodeError"] = _localizationService.GetResource("Plugin.DiscountCode.InvalidDiscountCode");
                return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
            }
            if (tblDiscountCode.ExpiryDate != null && tblDiscountCode.ExpiryDate < DateTime.Now)
            {
                TempData["DiscountCodeIsExpired"] = _localizationService.GetResource("Plugin.DiscountCode.InvalidDiscountCode");
                return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
            }
            if (tblDiscountCode.MaxNumberOfUsage != null && tblDiscountCode.MaxNumberOfUsage <= await _dbContext.InvoicesDiscountCode.CountAsync(p => p.DiscountCode == tblDiscountCode.DiscountCode && p.Invoice.Status == InvoiceStatus.Paid))
            {
                TempData["DiscountCodeIsExpired"] = _localizationService.GetResource("Plugin.DiscountCode.InvalidDiscountCode");
                return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
            }

            await _dbContext.InvoicesDiscountCode.Where(p => p.InvoiceId == model.InvoiceId).DeleteAsync();
            _dbContext.InvoicesDiscountCode.Add(new TblInvoicesDiscountCode()
            {
                DiscountCode = tblDiscountCode.DiscountCode,
                InvoiceId = model.InvoiceId
            });
            await _dbContext.SaveChangesAsync();
            await _invoiceService.ApplyInvoiceDiscountsAsync(tblInvoice.Id);
            return RedirectToAction("GetInvoiceTablePartial", "Invoice", new { invoiceId = model.InvoiceId });
        }
    }
}
