using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Profile;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.Products;
using Devesprit.Services.Users;
using Elmah;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class PurchaseController : BaseController
    {
        private readonly IUserGroupsService _userGroupsService;
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;

        public PurchaseController(IUserGroupsService userGroupsService, 
            IInvoiceService invoiceService, 
            IProductService productService,
            IProductModelFactory productModelFactory,
            ILocalizationService localizationService,
            IProductCheckoutAttributesService productCheckoutAttributesService)
        {
            _userGroupsService = userGroupsService;
            _invoiceService = invoiceService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _localizationService = localizationService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
        }
         
        public virtual async Task<ActionResult> UpgradeAccount()
        {
            var user = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var model = new UpgradeAccountModel()
            {
                CurrentUser = user
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> UpgradeAccount(int selectedUserGroupId, Guid? invoiceId)
        {
            var userGroup = await _userGroupsService.FindByIdAsync(selectedUserGroupId);
            var currentUser = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            if (userGroup == null)
            {
                return View("Error");
            }

            if (User.IsInRole("Admin") && invoiceId != null)
            {
                await _invoiceService.AddItemToInvoiceAsync(
                    InvoiceDetailsItemType.SubscriptionPlan,
                    string.Format(_localizationService.GetResource("UpgradeUserAccountTo"), userGroup.GetLocalized(p => p.GroupName)),
                    Url.Action("UpgradeAccount", null, null, Request.Url.Scheme),
                    userGroup.Id,
                    await _userGroupsService.CalculatePlanPriceForUserAsync(userGroup.Id, currentUser),
                    1,
                    invoiceId);
                return RedirectToAction("index", "Invoice", new {id = invoiceId});
            }
            else
            {
                await _invoiceService.AddItemToInvoiceAsync(
                    InvoiceDetailsItemType.SubscriptionPlan,
                    string.Format(_localizationService.GetResource("UpgradeUserAccountTo"), userGroup.GetLocalized(p => p.GroupName)),
                    Url.Action("UpgradeAccount", null, null, Request.Url.Scheme),
                    userGroup.Id,
                    await _userGroupsService.CalculatePlanPriceForUserAsync(userGroup.Id, currentUser),
                    1);
                return RedirectToAction("index", "Invoice");
            }
        }

        public virtual async Task<ActionResult> PurchaseProductWizard(int productId, Guid? invoiceId)
        {
            var product = await _productService.FindByIdAsync(productId);
            var currentUser = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var isAdmin = User.IsInRole("Admin");

            if (product == null || (!product.Published && !isAdmin))
                return View("Partials/_PageNotFound"); // product id is invalid or not published

            if (invoiceId != null) ViewBag.InvoiceId = invoiceId.ToString();
            return View(_productModelFactory.PrepareProductModel(product, currentUser, Url));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> AddProductToInvoice(int productId, bool? upgradeAttributes, FormCollection collection, Guid? invoiceId)
        {
            if (!User.IsInRole("Admin") && invoiceId != null)
            {
                invoiceId = null;
            }

            var product = await _productService.FindByIdAsync(productId);
            if (product == null)
            {
                return Content(_localizationService.GetResource("AnServerErrorOccurred"));
            }

            var currentUser = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());

            if (upgradeAttributes != true)
            {
                await _invoiceService.AddItemToInvoiceAsync(
                   InvoiceDetailsItemType.Product,
                   product.GetLocalized(p => p.Title),
                   Url.Action("Index", "Product", new { slug = product.Slug }, Request.Url.Scheme),
                   productId,
                   _productService.CalculateProductPriceForUser(product, currentUser),
                   1,
                   invoiceId);
            }

            try
            {
                foreach (string key in collection.Keys)
                {
                    if (key.StartsWith("attr-") || key.StartsWith("opt-"))
                    {
                        var attrId = int.Parse(key.Replace("attr-", ""));
                        var attribute = product.CheckoutAttributes.FirstOrDefault(p => p.Id == attrId);
                        if (attribute.AttributeType == ProductCheckoutAttributeType.CheckBoxList ||
                            attribute.AttributeType == ProductCheckoutAttributeType.RadioButtonList ||
                            attribute.AttributeType == ProductCheckoutAttributeType.DropDownList)
                        {
                            var optIds = collection[key];
                            if (!string.IsNullOrWhiteSpace(optIds))
                            {
                                foreach (var optIdstr in optIds.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    var optId = int.Parse(optIdstr);
                                    var option = attribute.Options.FirstOrDefault(p => p.Id == optId);

                                    await _invoiceService.AddItemToInvoiceAsync(
                                        InvoiceDetailsItemType.ProductAttributeOption,
                                        attribute.GetLocalized(p => p.Name) + ": " + option.GetLocalized(p => p.Name),
                                        Url.Action("Index", "Product", new { slug = product.Slug }, Request.Url.Scheme),
                                        optId,
                                        await _productCheckoutAttributesService.CalculateAttributeOptionPriceForUserAsync(
                                            option.Id, currentUser),
                                        1,
                                        invoiceId);
                                }
                            }
                        }
                        else if (attribute.AttributeType == ProductCheckoutAttributeType.NumberBox)
                        {
                            if (int.TryParse(collection[key], out int value))
                            {
                                await _invoiceService.AddItemToInvoiceAsync(
                                    InvoiceDetailsItemType.ProductAttribute,
                                    attribute.GetLocalized(p => p.Name),
                                    Url.Action("Index", "Product", new { slug = product.Slug }, Request.Url.Scheme),
                                    attrId,
                                    attribute.UnitPrice,
                                    value,
                                    invoiceId);
                            }
                        }
                        else
                        {
                            var value = collection[key];
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                await _invoiceService.AddItemToInvoiceAsync(
                                    InvoiceDetailsItemType.ProductAttribute,
                                    attribute.GetLocalized(p => p.Name) + ": " + Environment.NewLine + "<small>" + value + "</small>",
                                    Url.Action("Index", "Product", new { slug = product.Slug }, Request.Url.Scheme),
                                    attrId,
                                    0,
                                    0,
                                    invoiceId);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(ex, System.Web.HttpContext.Current));
                return Content(_localizationService.GetResource("AnServerErrorOccurred"));
            }
            
            return Content("OK");
        }
    }
}