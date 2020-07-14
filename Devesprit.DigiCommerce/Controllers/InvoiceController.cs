using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Invoice;
using Devesprit.Services.Invoice;
using Devesprit.Services.PaymentGateway;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.ActionResults;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
    [OutputCache(NoStore = true, Duration = 0)]
    public partial class InvoiceController : BaseController
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceModelFactory _invoiceModelFactory;
        private readonly IPaymentGatewayManager _paymentGatewayManager;
        private readonly ILocalizationService _localizationService;

        public InvoiceController(IInvoiceService invoiceService,
            IInvoiceModelFactory invoiceModelFactory,
            IPaymentGatewayManager paymentGatewayManager,
            ILocalizationService localizationService)
        {
            _invoiceService = invoiceService;
            _invoiceModelFactory = invoiceModelFactory;
            _paymentGatewayManager = paymentGatewayManager;
            _localizationService = localizationService;
        }

        public virtual async Task<ActionResult> Index(Guid? id)
        {
            var model = id == null
                ? await _invoiceService.GetUserCurrentInvoiceAsync()
                : await _invoiceService.FindByIdAsync(id.Value);

            return model == null
                ? View("PageNotFound")
                : View(await _invoiceModelFactory.PrepareInvoiceModelAsync(model));
        }
         
        public virtual async Task<ActionResult> Print(Guid id)
        {
            var model = await _invoiceService.FindByIdAsync(id);

            return model == null
                ? View("PageNotFound")
                : View(await _invoiceModelFactory.PrepareInvoiceModelAsync(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(Guid Id, string paymentMethod, InvoiceBillingAddressModel address)
        {
            var invoice = await _invoiceService.FindByIdAsync(Id);
            if (invoice == null)
            {
                return View("PageNotFound");
            }

            if (invoice.InvoiceDetails.Count == 0)
            {
                ViewBag.Message = _localizationService.GetResource("ShoppingCartEmpty");
                return View(await _invoiceModelFactory.PrepareInvoiceModelAsync(invoice));
            }

            if (CurrentSettings.GetBillingAddressForInvoice)
            {
                if (!ModelState.IsValid)
                {
                    return View(await _invoiceModelFactory.PrepareInvoiceModelAsync(invoice));
                }
            }
            else
            {
                if (!ModelState.IsValidField("Id") || !ModelState.IsValidField("paymentMethod"))
                {
                    return View(await _invoiceModelFactory.PrepareInvoiceModelAsync(invoice));
                }
            }

            if (CurrentSettings.GetBillingAddressForInvoice)
            {
                await _invoiceService.AddUpdateBillingAddressAsync(Id, _invoiceModelFactory.PrepareTblInvoiceBillingAddress(address));
            }

            var paymentGateway = _paymentGatewayManager.FindPaymentMethodBySystemName(paymentMethod);

            if (invoice.ComputeInvoiceTotalAmount() <= 0)
            {
                await _invoiceService.SetGatewayNameAndTokenAsync(Id, paymentGateway.PaymentGatewayName,
                    paymentGateway.PaymentGatewaySystemName, "-", WorkContext.CurrentCurrency.Id);
                return RedirectToAction("VerifyPayment", new { id = Id });
            }

            var result =
                await paymentGateway.RequestForPaymentUrl(
                    Url.Action("VerifyPayment", "Invoice", new { id = Id }, Request.Url.Scheme),
                    invoice);

            if (result.IsSuccess)
            {
                await _invoiceService.SetGatewayNameAndTokenAsync(Id, paymentGateway.PaymentGatewayName,
                    paymentGateway.PaymentGatewaySystemName, result.Token, WorkContext.CurrentCurrency.Id);
                if (result.PostDate != null)
                {
                    return new RedirectAndPostActionResult(result.RedirectUrl, result.PostDate);
                }

                return Redirect(result.RedirectUrl);
            }

            ViewBag.Message = result.ErrorMessage;
            return View(await _invoiceModelFactory.PrepareInvoiceModelAsync(invoice));
        }

        [HttpPost]
        public virtual async Task<ActionResult> RemoveInvoiceItem(Guid invoiceId, int detailId)
        {
            if (await _invoiceService.UserCanEditInvoiceAsync(invoiceId, User.Identity.GetUserId()))
            {
                await _invoiceService.RemoveItemAsync(detailId);
            }

            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);

            if (tblInvoice == null || !tblInvoice.InvoiceDetails.Any())
            {
                await _invoiceService.DeleteAsync(invoiceId);
                if (User.IsInRole("Admin"))
                {
                    return Content("<script>window.location.href = '" +
                                   Url.Action("Index", "ManageInvoices", new { area = "Admin" }) + "'</script>");
                }
                return Content("<script>window.location.href = '" + Url.Action("Index", "Profile") + "'</script>");
            }

            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        public virtual async Task<ActionResult> GetInvoiceTablePartial(Guid invoiceId)
        {
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return PartialView("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        [HttpPost]
        public virtual async Task<ActionResult> IncreaseInvoiceItemQty(Guid invoiceId, int detailId)
        {
            if (await _invoiceService.UserCanEditInvoiceAsync(invoiceId, User.Identity.GetUserId()))
                await _invoiceService.IncreaseItemQtyAsync(detailId);
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        [HttpPost]
        public virtual async Task<ActionResult> DecreaseInvoiceItemQty(Guid invoiceId, int detailId)
        {
            if (await _invoiceService.UserCanEditInvoiceAsync(invoiceId, User.Identity.GetUserId()))
                await _invoiceService.DecreaseItemQtyAsync(detailId);
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        public virtual async Task<ActionResult> VerifyPayment(Guid id)
        {
            var invoice = await _invoiceService.FindByIdAsync(id);
            if (invoice == null || invoice.Status == InvoiceStatus.Paid)
            {
                return View("PageNotFound");
            }
            ViewBag.InvoiceId = id;
            if (invoice.ComputeInvoiceTotalAmount() > 0)
            {
                var paymentGateway = _paymentGatewayManager.FindPaymentMethodBySystemName(invoice.PaymentGatewaySystemName);
                if (paymentGateway == null)
                {
                    return View("Error");
                }

                var result = await paymentGateway.VerifyPayment(invoice);
                if (result.IsSuccess)
                {
                    await _invoiceService.CheckoutInvoiceAsync(invoice.Id, result.TransactionId);
                }
                else
                {
                    //error
                    ViewBag.Error = result.ErrorMessage;
                }
            }

            return View();
        }

        #region Administration

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceStatus")]
        public virtual async Task<ActionResult> SetInvoiceStatus(Guid invoiceId, InvoiceStatus Status)
        {
            await _invoiceService.SetStatusAsync(invoiceId, Status);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoicePaymentDate")]
        public virtual async Task<ActionResult> SetInvoicePaymentDate(Guid invoiceId, string PaymentDate)
        {
            if (DateTime.TryParseExact(PaymentDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentUICulture,
                DateTimeStyles.None, out DateTime paymentDate))
            {
                await _invoiceService.SetPaymentDateAsync(invoiceId, paymentDate);
            }
            else
            {
                await _invoiceService.SetPaymentDateAsync(invoiceId, null);
            }
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceItemExpirationDate")]
        public virtual async Task<ActionResult> SetInvoiceItemExpirationDate(int itemId, string expDate)
        {
            if (DateTime.TryParseExact(expDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.CurrentUICulture,
                DateTimeStyles.None, out DateTime expirationDate))
            {
                await _invoiceService.SetItemExpirationAsync(itemId, expirationDate);
            }
            else
            {
                await _invoiceService.SetItemExpirationAsync(itemId, null);
            }
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceItemLicenseCode")]
        public virtual async Task<ActionResult> SetInvoiceItemLicenseCode(int itemId, string license)
        {
            await _invoiceService.SetItemLicenseAsync(itemId, license);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceUser")]
        public virtual async Task<ActionResult> SetInvoiceUser(Guid invoiceId, string UserName)
        {
            var user = await UserManager.FindByNameAsync(UserName);
            if (user == null)
            {
                return Json(new { response = _localizationService.GetResource("RequestedUserNotFound") });
            }
            await _invoiceService.SetUserIdAsync(invoiceId, user.Id);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceItemUnitPrice")]
        public virtual async Task<ActionResult> SetInvoiceItemUnitPrice(Guid invoiceId, int itemId, double? itemUnitPrice)
        {
            if (itemUnitPrice == null)
            {
                itemUnitPrice = 0;
            }
            var currentCurrency = WorkContext.CurrentCurrency;
            if (!currentCurrency.IsMainCurrency)
            {
                itemUnitPrice = itemUnitPrice / currentCurrency.ExchangeRate;
            }
            
            await _invoiceService.SetItemUnitPriceAsync(itemId, itemUnitPrice.Value);
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceDiscount")]
        public virtual async Task<ActionResult> SetInvoiceDiscount(Guid invoiceId, double? discountAmount, string discountDescription)
        {
            if (discountAmount == null)
            {
                discountAmount = 0;
            }
            var currentCurrency = WorkContext.CurrentCurrency;
            if (!currentCurrency.IsMainCurrency)
            {
                discountAmount = discountAmount / currentCurrency.ExchangeRate;
            }

            await _invoiceService.SetDiscountAsync(invoiceId, discountAmount.Value, discountDescription);
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceTax")]
        public virtual async Task<ActionResult> SetInvoiceTax(Guid invoiceId, double? taxAmount, string taxDescription)
        {
            if (taxAmount == null)
            {
                taxAmount = 0;  
            }
            var currentCurrency = WorkContext.CurrentCurrency;
            if (!currentCurrency.IsMainCurrency)
            {
                taxAmount = taxAmount / currentCurrency.ExchangeRate;
            }

            await _invoiceService.SetTaxAsync(invoiceId, taxAmount.Value, taxDescription);
            var tblInvoice = await _invoiceService.FindByIdAsync(invoiceId);
            return View("Partials/_InvoiceTable", await _invoiceModelFactory.PrepareInvoiceModelAsync(tblInvoice));
        }

        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_AddNewItemToInvoiceManually")]
        public virtual ActionResult AddNewItemToInvoiceManually(Guid invoiceId)
        {
            return View("Partials/_AddNewItemToInvoiceManually", invoiceId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_AddNewItemToInvoiceManually")]
        public virtual async Task<ActionResult> AddNewItemToInvoiceManually(Guid invoiceId, string itemName, string itemHomePage, int qty, double? unitPrice)
        {
            if (unitPrice == null)
            {
                unitPrice = 0;
            }
            var currentCurrency = WorkContext.CurrentCurrency;
            if (!currentCurrency.IsMainCurrency)
            {
                unitPrice = unitPrice / currentCurrency.ExchangeRate;
            }

            await _invoiceService.AddItemToInvoiceAsync(
                InvoiceDetailsItemType.Other,
                itemName,
                itemHomePage,
                0,
                unitPrice.Value,
                qty,
                invoiceId);
            return RedirectToAction("Index", new { id = invoiceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_UpdateInvoiceBillingAddress")]
        public virtual async Task<ActionResult> UpdateInvoiceBillingAddress(Guid invoiceId, InvoiceBillingAddressModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    response = ModelState.Values.FirstOrDefault(p => p.Errors.Any()).Errors.FirstOrDefault()
                        .ErrorMessage
                });
            }

            var record = _invoiceModelFactory.PrepareTblInvoiceBillingAddress(model);
            await _invoiceService.AddUpdateBillingAddressAsync(invoiceId, record);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_SetInvoiceNote")]
        public virtual async Task<ActionResult> SetInvoiceNote(Guid invoiceId, string note, bool isForAdmin)
        {
            await _invoiceService.SetInvoiceNoteAsync(invoiceId, note, isForAdmin);
            return Json(new { response = "OK" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        [UserHasPermission( "ManageInvoices_CheckoutInvoice")]
        public virtual async Task<ActionResult> CheckoutInvoice(Guid invoiceId)
        {
            await _invoiceService.CheckoutInvoiceAsync((await _invoiceService.FindByIdAsync(invoiceId)).Id, "");
            return RedirectToAction("Index", new { id = invoiceId });
        }

        #endregion
    }
}