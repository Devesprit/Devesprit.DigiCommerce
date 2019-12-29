using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.PaymentGateway;
using Devesprit.Services.Users;
using Plugin.PaymentMethod.Zarinpal.Models;
using Plugin.PaymentMethod.Zarinpal.Properties;

namespace Plugin.PaymentMethod.Zarinpal
{
    public partial class ZarinpalPaymentMethodPlugin : BasePlugin, IPaymentMethod
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUserRolesService _userRolesService;

        public ZarinpalPaymentMethodPlugin(ILocalizationService localizationService, ISettingService settingService, IUserRolesService userRolesService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _userRolesService = userRolesService;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ZarinPalGateWay";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.PaymentMethod.Zarinpal.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            _settingService.SaveSetting(new ZarinPalSettingsModel());

            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.InvoiceNumber", "Invoice No. : {0}", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantCodeIsInvalid", "Please specify your ZarinPal merchant ID in the configuration section.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentGatewayName", "ZarinPal Payment Gateway (for Iranian Users)", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_1", "The submitted information is incomplete.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_11", "This request was not found.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_12", "You are not allowed to edit this request.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_2", "The receptor's IP or Merchant Code is incorrect.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_21", "No payment processing was found for this transaction.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_22", "Transaction failed!", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_3", "Due to the limitations of Shaparak payment system, the requested payment is not possible.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_33", "The transaction amount does not match the payment amount.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_34", "Split transaction limit has been exceeded in terms of number or digits.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_4", "The acceptance level is lower than Silver level.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_40", "Access to the relevant method is denied.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_41", "The information submitted for AdditionalData is invalid.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_42", "The transaction ID will be valid between 30 minutes to 45 days.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_54", "The request has been archived.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult100", "The operation has been completed successfully.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult101", "The payment has been successfully processed and the PaymentVerification has already been done.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResultUnknown", "Unknown Error.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ServiceUnavailable", "ZarinPal service is unavailable right now! Please try again after a few moments.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusFieldIsNotOK", "Unfortunately, the payment processing has not been completed, reason: Transaction was cancelled by the user.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusOrAuthorityInvalid", "An error occurred during the confirmation process! Reason: Status or Token is incorrect.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantID", "Your Zarinpal Merchant ID", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebserviceUrl", "Zarinpal Webservice Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalPaymentPageUrl", "Zarinpal Payment Page URL", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebSite", "Go to Zarinpal.com", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalConfig", "Zarinpal Payment Gateway Configurations", "en");

            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.InvoiceNumber", "فاکتور شماره : {0}", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantCodeIsInvalid", "لطفا کد درگاه پرداخت (مرچنت کد) را در بخش تنظیمات مشخص کنید.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentGatewayName", "درگاه پرداخت زرین پال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_1", "اطلاعات ارسال شده ناقص است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_11", "درخواست مورد نظر یافت نشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_12", "امکان ویرایش درخواست میسر نمي باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_2", "IP و يا مرچنت كد پذیرنده صحیح نیست.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_21", "هيچ نوع عملیات مالی برای اين تراکنش یافت نشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_22", "تراکنش نا موفق مي باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_3", "با توجه به محدودیت هاي شاپرک امکان پرداخت با رقم درخواست شده میسر نمي باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_33", "رقم تراکنش با رقم پرداخت شده مطابقت ندارد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_34", "سقف تقسیم تراکنش از لحاظ تعداد يا رقم عبور نموده است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_4", "سطح تایید پذیرنده پایین تر از سطح نقره اي است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_40", "اجازه دسترسی به متد مربوطه وجود ندارد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_41", "اطلاعات ارسال شده مربوط به AdditionalData غير معتبر مي باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_42", "مدت زمان معتبر طول عمر شناسه پرداخت بين 30 دقیقه تا 45 روز مي باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_54", "درخواست مورد نظر آرشیو شده است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult100", "عملیات با موفقیت انجام گردیده است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult101", "عملیات پرداخت موفق بوده و قبلا PaymentVerification تراکنش انجام شده است.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResultUnknown", "خطای نامشخص.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ServiceUnavailable", "متاسفانه در حال حاضر سرویس زرینپال خارج از دسترس است لطفا دقایقی بعد دوباره امتحان کنید.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusFieldIsNotOK", "متاسفانه عملیات پرداخت تکمیل نگردید، دلیل : Status برابر با OK نبود.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusOrAuthorityInvalid", "متاسفانه در عملیات تائید پرداخت شما خطایی رخ داده است، دلیل : Status و یا Authority صحیح نمی باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantID", "کد مرچنت زرینپال شما", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebserviceUrl", "آدرس وب سرویس زرینپال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalPaymentPageUrl", "آدرس صفحه پرداخت زرینپال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebSite", "وب سایت Zarinpal", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalConfig", "تنظیمات درگاه پرداخت زرین پال", "fa");

            _userRolesService.AddAccessAreas(new TblUserAccessAreas("Plugins", "ZarinpalGateWayConfig", "Plugin.PaymentMethod.Zarinpal.ZarinpalConfig"));
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<ZarinPalSettingsModel>();

            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.InvoiceNumber");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantCodeIsInvalid");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentGatewayName");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_1");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_11");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_12");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_2");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_21");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_22");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_3");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_33");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_34");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_4");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_40");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_41");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_42");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_54");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult100");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResult101");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.PaymentResultUnknown");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ServiceUnavailable");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusFieldIsNotOK");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.StatusOrAuthorityInvalid");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.MerchantID");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebserviceUrl");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalPaymentPageUrl");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebSite");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.Zarinpal.ZarinpalConfig");

            _userRolesService.DeleteAccessAreas("ZarinpalGateWayConfig");
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Uninstall();
        }

        public virtual async Task<PaymentRequestResult> RequestForPaymentUrl(string callbackUrl, TblInvoices invoice)
        {
            var settings = await _settingService.LoadSettingAsync<ZarinPalSettingsModel>();
            var currentCurrency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            var amount = invoice.ComputeInvoiceTotalAmount();
            if (string.Compare(currentCurrency.IsoCode, "irr", StringComparison.OrdinalIgnoreCase) == 0)
            {
                amount = amount / 10;
            }
            var buyerEmail = "";
            var buyerPhoneNumber = "";
            var paymentDesc = string.Format(_localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.InvoiceNumber"), invoice.Id.ToString("N").ToUpper());
            if (invoice.User != null)
            {
                buyerEmail = invoice.User.Email;
                buyerPhoneNumber = invoice.User.PhoneNumber;
            }

            if (string.IsNullOrWhiteSpace(settings.MerchantId))
            {
                return new PaymentRequestResult()
                {
                    ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.MerchantCodeIsInvalid")
                };
            }

            var result = new PaymentRequestResult();
            var zResult = 0;
            var authority = "";
            var connectionError = false;
            await Task.Run(() =>
            {
                try
                {
                    using (var zarinpal = new com.zarinpal.www.PaymentGatewayImplementationService()
                    {
                        Url = settings.ZarinpalWebserviceUrl
                    })
                    {
                        zResult = zarinpal.PaymentRequest(settings.MerchantId,
                            (int)amount,
                            paymentDesc,
                            buyerEmail,
                            buyerPhoneNumber,
                            callbackUrl,
                            out authority);
                    }
                }
                catch
                {
                    connectionError = true;
                }
            });

            if (zResult == 100)
            {
                result.RedirectUrl = string.Format(settings.ZarinpalPaymentPageUrl, authority);
                result.Token = authority;
                result.IsSuccess = true;
            }
            else
            {
                result.ErrorMessage = connectionError ?
                    _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.ServiceUnavailable") :
                    $"[{zResult}] : {GetErrorDes(zResult)}";
            }

            return result;
        }

        public virtual async Task<VerifyPaymentResult> VerifyPayment(TblInvoices invoice)
        {
            var settings = await _settingService.LoadSettingAsync<ZarinPalSettingsModel>();
            var currentCurrency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            var amount = invoice.ComputeInvoiceTotalAmount();
            if (string.Compare(currentCurrency.IsoCode, "irr", StringComparison.OrdinalIgnoreCase) == 0)
            {
                amount = amount / 10;
            }

            var result = new VerifyPaymentResult();
            var token = invoice.PaymentGatewayToken;

            var request = HttpContext.Current.Request;
            if (string.IsNullOrWhiteSpace(request.QueryString["Status"]) ||
                string.IsNullOrWhiteSpace(request.QueryString["Authority"]))
            {
                result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.StatusOrAuthorityInvalid");
                return result;
            }
            if (!request.QueryString["Status"].Equals("OK"))
            {
                result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.StatusFieldIsNotOK");
                return result;
            }

            var zResult = 0;
            long refId = 0;
            var connectionError = false;
            await Task.Run(() =>
            {
                try
                {
                    using (var zarinpal = new com.zarinpal.www.PaymentGatewayImplementationService()
                    {
                        Url = settings.ZarinpalWebserviceUrl
                    })
                    {
                        zResult = zarinpal.PaymentVerification(settings.MerchantId,
                            token,
                            (int)amount,
                            out refId);
                    }
                }
                catch
                {
                    connectionError = true;
                }
            });

            if (zResult == 100)
            {
                result.IsSuccess = true;
                result.TransactionId = refId.ToString();
            }
            else
            {
                result.ErrorMessage = connectionError ?
                    _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.ServiceUnavailable") :
                    $"[{zResult}] : {GetErrorDes(zResult)}";
            }

            return result;
        }

        protected virtual string GetErrorDes(int error)
        {
            switch (error)
            {
                case -1: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_1");
                case -2: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_2");
                case -3: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_3");
                case -4: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_4");
                case -11: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_11");
                case -12: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_12");
                case -21: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_21");
                case -22: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_22");
                case -33: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_33");
                case -34: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_34");
                case -40: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_40");
                case -41: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_41");
                case -42: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_42");
                case -54: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult_54");
                case 100: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult100");
                case 101: return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResult101");
                default:
                    return _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentResultUnknown");
            }
        }

        public string PaymentGatewayName => _localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.PaymentGatewayName");
        public string PaymentGatewaySystemName => "zarinpal";
        public List<string> AcceptedCurrenciesIso => new List<string>() { "irr" /*ریال ایران*/, "irt" /*تومان ایران*/};
        public byte[] PaymentGatewayIcon
        {
            get
            {
                if (_paymentGatewayIcon != null && _paymentGatewayIcon.Length > 0)
                {
                    return _paymentGatewayIcon;
                }
                using (var stream = new MemoryStream())
                {
                    Resources.logo.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    _paymentGatewayIcon = stream.ToArray();
                    return _paymentGatewayIcon;
                }
            }
        }

        private byte[] _paymentGatewayIcon;
    }
}
