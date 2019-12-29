using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Currency;
using Devesprit.Services.Discounts;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.Users;
using Devesprit.Services.Widget;
using Plugin.DiscountCode.DB;

namespace Plugin.DiscountCode
{
    public partial class DiscountCodePlugin : BasePlugin, IWidgetPlugin, IDiscountProcessor
    {
        private readonly DiscountCodeDbContext _dbContext;
        private readonly ILocalizationService _localizationService;
        private readonly IUserRolesService _userRolesService;

        public DiscountCodePlugin(DiscountCodeDbContext dbContext, ILocalizationService localizationService, IUserRolesService userRolesService)
        {
            _dbContext = dbContext;
            _localizationService = localizationService;
            _userRolesService = userRolesService;
        }

        public virtual IList<string> GetWidgetZones()
        {
            var zoneLists = new List<string>(){ "Invoice_Table_Summary_Start" };
            return zoneLists;
        }

        public virtual void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "DiscountCode";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.DiscountCode.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "DiscountCode";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.DiscountCode.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCode", "Discount Code", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.ManageDiscountCodes", "Manage Discount Codes", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.MaxNumberOfUsage", "Max Number of Usage", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.AmountIsPercentage", "Amount is Percentage", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.InvalidDiscountCode", "The discount code is invalid", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCodeIsExpired", "This code has been expired", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.NumberOfUsed", "Number of used", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCodeDuplicateError", "Entered discount code is duplicate, please enter another code", "en");
            
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCode", "کد تخفیف", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.ManageDiscountCodes", "مدیریت کدهای تخفیف", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.MaxNumberOfUsage", "حداکثر تعداد استفاده", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.AmountIsPercentage", "مقدار بر اساس درصد", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.InvalidDiscountCode", "کد تخفیف وارد شده صحیح نمی باشد", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCodeIsExpired", "کد تخفیف وارد شده منقضی شده است", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.NumberOfUsed", "تعداد استفاده شده", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.DiscountCode.DiscountCodeDuplicateError", "کد تخفیف وارد شده تکراری می باشد، لطفا کد دیگری وارد کنید", "fa");

            _userRolesService.AddAccessAreas(new TblUserAccessAreas("Plugins", "DevespritDiscountCodeConfig", "Plugin.DiscountCode.ManageDiscountCodes"));
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugin.DiscountCode.DiscountCode");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.ManageDiscountCodes");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.MaxNumberOfUsage");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.AmountIsPercentage");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.InvalidDiscountCode");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.DiscountCodeIsExpired");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.NumberOfUsed");
            this.DeletePluginLocaleResource("Plugin.DiscountCode.DiscountCodeDuplicateError");

            _userRolesService.DeleteAccessAreas("DevespritDiscountCodeConfig");
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Uninstall();
        }

        public DiscountProcessorResult ProcessorInvoice(TblInvoices invoice)
        {
            var result = new DiscountProcessorResult();
            if (invoice == null)
            {
                return result;
            }

            var appliedDiscountCode = _dbContext.InvoicesDiscountCode.FirstOrDefault(p => p.InvoiceId == invoice.Id);
            if (appliedDiscountCode == null)
            {
                return result;
            }

            var discountCode =
                _dbContext.DiscountCode.FirstOrDefault(p => p.DiscountCode == appliedDiscountCode.DiscountCode.Trim());
            if (discountCode == null)
            {
                return result;
            }
            if (discountCode.ExpiryDate != null && discountCode.ExpiryDate < DateTime.Now)
            {
                return result;
            }
            if (discountCode.MaxNumberOfUsage != null && discountCode.MaxNumberOfUsage <= _dbContext.InvoicesDiscountCode.Count(p=> p.DiscountCode == appliedDiscountCode.DiscountCode && p.Invoice.Status == InvoiceStatus.Paid))
            {
                return result;
            }

            if (discountCode.IsPercentage)
            {
                var invoiceTotal = invoice.ComputeInvoiceTotalAmount(false, false);
                result.DiscountAmountInMainCurrency = (invoiceTotal * discountCode.DiscountAmount) / 100; 
            }
            else
            {
                result.DiscountAmountInMainCurrency = discountCode.DiscountAmount;
            }

            result.Apply = true;
            result.DiscountDescription = (string.IsNullOrWhiteSpace(discountCode.DiscountCodeTitle)
                                             ? _localizationService.GetResource("Plugin.DiscountCode.DiscountCode")
                                             : discountCode.DiscountCodeTitle) +
                                         $" ({(discountCode.IsPercentage ? discountCode.DiscountAmount.ToString("N2")+"%" : discountCode.DiscountAmount.ExchangeCurrencyStr(true))})";

            return result;
        }

        public int Order => 0;
    }
}
