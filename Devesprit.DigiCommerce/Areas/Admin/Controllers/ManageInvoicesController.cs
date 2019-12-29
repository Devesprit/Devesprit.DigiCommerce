using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Invoice;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Microsoft.AspNet.Identity;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageInvoices")]
    public partial class ManageInvoicesController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ILocalizationService _localizationService;

        public ManageInvoicesController(IInvoiceService invoiceService,
            ILocalizationService localizationService)
        {
            _invoiceService = invoiceService;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult Grid(string discountCode)
        {
            if (!string.IsNullOrWhiteSpace(discountCode))
            {
                //Filter by category
                ViewBag.DataSource = Url.Action("GridDataSource", new { discountCode = discountCode });
            }
            else
            {
                ViewBag.DataSource = Url.Action("GridDataSource");
            }

            return PartialView();
        }

        [HttpPost]
        [UserHasPermission("ManageInvoices_Add")]
        public virtual async Task<ActionResult> CreateNew()
        {
            var id = await _invoiceService.AddAsync(new TblInvoices()
            {
                CreateDate = DateTime.Now,
                Status = InvoiceStatus.Pending,
                UserId = User.Identity.GetUserId(),
            });
            return Content(id.ToString());
        }

        [HttpPost]
        [UserHasPermission("ManageInvoices_Delete")]
        public virtual async Task<ActionResult> Delete(Guid[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _invoiceService.DeleteAsync(key);
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
            var query = _invoiceService.GetAsQueryable();
            
            var dataSource = query.Select(p => new
            {
                p.Id,
                Status = p.Status + "",
                ItemsCount = p.InvoiceDetails.Count,
                BillingAddress = p.BillingAddress.FirstOrDefault().StreetAddress ?? "-",
                p.CreateDate,
                p.PaymentDate,
                PaymentGatewayTransactionId = p.PaymentGatewayTransactionId ?? "-",
                PaymentGatewayName = p.PaymentGatewayName ?? "-",
                UserName = p.User.UserName ?? "-",
                Currency = p.Currency.CurrencyName ?? "-",
                p.DiscountAmount,
                invoice = p
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList().Select(p => new
            {
                p.Id,
                p.Status,
                p.ItemsCount,
                p.BillingAddress,
                p.CreateDate,
                p.PaymentDate,
                p.PaymentGatewayTransactionId,
                p.PaymentGatewayName,
                p.UserName,
                p.Currency,
                p.DiscountAmount,
                Total = p.invoice.ComputeInvoiceTotalAmount(false)
            });

            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}