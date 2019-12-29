using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
using PayPal.PayPalAPIInterfaceService;
using PayPal.PayPalAPIInterfaceService.Model;
using Plugin.PaymentMethod.PayPal.Models;
using Plugin.PaymentMethod.PayPal.Properties;

namespace Plugin.PaymentMethod.PayPal
{
    public partial class PayPalPaymentMethodPlugin : BasePlugin, IPaymentMethod
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUserRolesService _userRolesService;

        public PayPalPaymentMethodPlugin(ILocalizationService localizationService, ISettingService settingService, IUserRolesService userRolesService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _userRolesService = userRolesService;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PayPalGateWay";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.PaymentMethod.PayPal.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            _settingService.SaveSetting(new PayPalSettingsModel());

            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvoiceNumber", "Invoice No. : {0}", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvalidConfig", "Please specify your PayPal APIUserName & APIPassword &  APISignature in the configuration section.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentGatewayName", "PayPal Payment Gateway", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentResultUnknown", "Unknown Error.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusFieldIsNotOK", "Unfortunately, the payment processing has not been completed, reason: Transaction was cancelled by the user.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusOrTokenInvalid", "An error occurred during the confirmation process! Reason: Status or Token is incorrect.", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIUserName", "PayPal API UserName", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIPassword", "PayPal API Password", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APISignature", "PayPal API Signature", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.EndpointUrl", "PayPal Endpoint Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentPageUrl", "PayPal Payment Page Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestMode", "Request Mode (live or sandbox)", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.ConnectionTimeout", "Connection Timeout", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestRetries", "Request Retries", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalConfig", "PayPal Payment Gateway Configurations", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalHeaderImageUrl", "Your Brand Logo Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalBrandName", "Your Brand Name", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.ServiceUnavailable", "PayPal service is unavailable right now! Please try again after a few moments.", "en");

            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvoiceNumber", "Invoice No. : {0}", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvalidConfig", "لطفا اطلاعات مورد نیاز درگاه پرداخت را در بخش تنظیمات مشخص کنید.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentGatewayName", "درگاه پرداخت پی پال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentResultUnknown", "خطای نامشخص.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusFieldIsNotOK", "متاسفانه عملیات پرداخت تکمیل نگردید، دلیل : Status برابر با OK نبود.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusOrTokenInvalid", "متاسفانه در عملیات تائید پرداخت شما خطایی رخ داده است، دلیل : Status و یا Token صحیح نمی باشد.", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIUserName", "PayPal API UserName", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIPassword", "PayPal API Password", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.APISignature", "PayPal API Signature", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.EndpointUrl", "PayPal Endpoint Url", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentPageUrl", "PayPal Payment Page URL", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestMode", "نوع درخواست (live or sandbox)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.ConnectionTimeout", "Connection Timeout", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestRetries", "تعداد تلاش", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalConfig", "تنظیمات درگاه پرداخت پی پال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalHeaderImageUrl", "Url لوگوی برند شما", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalBrandName", "نام برند شما", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.PaymentMethod.PayPal.ServiceUnavailable", "متاسفانه در حال حاضر سرویس پی پال خارج از دسترس است لطفا دقایقی بعد دوباره امتحان کنید.", "fa");

            _userRolesService.AddAccessAreas(new TblUserAccessAreas("Plugins", "PayPalGateWayConfig", "Plugin.PaymentMethod.PayPal.PayPalConfig"));
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<PayPalSettingsModel>();

            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvoiceNumber");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.InvalidConfig");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentGatewayName");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.ServiceUnavailable");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusFieldIsNotOK");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.StatusOrTokenInvalid");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIUserName");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.APIPassword");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.APISignature");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.EndpointUrl");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.PaymentPageUrl");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestMode");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.ConnectionTimeout");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.RequestRetries");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalConfig");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalHeaderImageUrl");
            this.DeletePluginLocaleResource("Plugin.PaymentMethod.PayPal.PayPalBrandName");

            _userRolesService.DeleteAccessAreas("PayPalGateWayConfig");
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Uninstall();
        }



        public virtual async Task<PaymentRequestResult> RequestForPaymentUrl(string callbackUrl, TblInvoices invoice)
        {
            var settings = await _settingService.LoadSettingAsync<PayPalSettingsModel>();
            var currentCurrency = DependencyResolver.Current.GetService<IWorkContext>().CurrentCurrency;
            var amount = invoice.ComputeInvoiceTotalAmount();

            
            var currencyCode = CurrencyCodeType.USD;
            if (currentCurrency.IsoCode.ToLower().Trim() == "eur")
            {
                currencyCode = CurrencyCodeType.EUR;
            }
            var buyerEmail = "";
            var paymentDesc = string.Format(_localizationService.GetResource("Plugin.PaymentMethod.PayPal.InvoiceNumber"), invoice.Id.ToString("N").ToUpper());
            if (invoice.User != null)
            {
                buyerEmail = invoice.User.Email;
            }

            if (string.IsNullOrWhiteSpace(settings.APIUserName) ||
                string.IsNullOrWhiteSpace(settings.APIPassword) ||
                string.IsNullOrWhiteSpace(settings.APISignature))
            {
                return new PaymentRequestResult()
                {
                    ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.PayPal.InvalidConfig")
                };
            }

            var result = new PaymentRequestResult();
            await Task.Run(() =>
            {
                try
                {
                    var ecDetails = new SetExpressCheckoutRequestDetailsType()
                    {
                        ReturnURL = AppendQueryStringToUrl(callbackUrl, "Status", "OK"),
                        CancelURL = AppendQueryStringToUrl(callbackUrl, "Status", "NOK"),
                        LocaleCode = "US",
                        BuyerEmail = buyerEmail,
                        InvoiceID = invoice.Id.ToString(),
                        OrderDescription = paymentDesc,
                        ShippingMethod = ShippingServiceCodeType.DOWNLOAD,
                        PaymentAction = PaymentActionCodeType.SALE,
                        OrderTotal = new BasicAmountType(currencyCode, amount.ToString()),
                        NoShipping = "1", //PayPal does not display shipping address fields and removes shipping information from the transaction.
                        ReqConfirmShipping = "0", //do not require the buyer's shipping address be a confirmed address.
                        AddressOverride = "0", //The PayPal pages should not display the shipping address.
                        cppHeaderImage = settings.PayPalHeaderImageUrl,
                        BrandName = settings.PayPalBrandName,
                    };

                    var request = new SetExpressCheckoutRequestType(ecDetails);
                    var wrapper = new SetExpressCheckoutReq { SetExpressCheckoutRequest = request };
                    var service = new PayPalAPIInterfaceServiceService(GetPayPalConfig());
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    var setEcResponse = service.SetExpressCheckout(wrapper);

                    if (setEcResponse.Ack == AckCodeType.SUCCESS)
                    {
                        result.RedirectUrl = settings.PaymentPageUrl + "_express-checkout&token=" +
                                             setEcResponse.Token;
                        result.Token = setEcResponse.Token;
                        result.IsSuccess = true;
                    }
                    else
                    {
                        foreach (var error in setEcResponse.Errors)
                        {
                            result.ErrorMessage +=
                                $"[{error.SeverityCode}] {error.ErrorCode} : {error.LongMessage} </br>";
                        }
                    }
                }
                catch
                {
                    result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.PayPal.ServiceUnavailable");
                }
            });

            return result;
        }

        public virtual async Task<VerifyPaymentResult> VerifyPayment(TblInvoices invoice)
        {
            var result = new VerifyPaymentResult();
            var token = invoice.PaymentGatewayToken;

            var request = HttpContext.Current.Request;
            if (string.IsNullOrWhiteSpace(request.QueryString["Status"]) ||
                string.IsNullOrWhiteSpace(request.QueryString["token"]))
            {
                result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.PayPal.StatusOrTokenInvalid");
                return result;
            }
            if (!request.QueryString["Status"].Equals("OK"))
            {
                result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.PayPal.StatusFieldIsNotOK");
                return result;
            }

            await Task.Run(() =>
            {
                try
                {
                    var service = new PayPalAPIInterfaceServiceService(GetPayPalConfig());
                    var getEcWrapper = new GetExpressCheckoutDetailsReq
                    {
                        GetExpressCheckoutDetailsRequest = new GetExpressCheckoutDetailsRequestType(token)
                    };
                    var getEcResponse = service.GetExpressCheckoutDetails(getEcWrapper);
                    var ecRequest = new DoExpressCheckoutPaymentRequestType();
                    var requestDetails = new DoExpressCheckoutPaymentRequestDetailsType();
                    ecRequest.DoExpressCheckoutPaymentRequestDetails = requestDetails;
                    requestDetails.PaymentDetails = getEcResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails;
                    requestDetails.Token = token;
                    requestDetails.PayerID = getEcResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID;
                    requestDetails.PaymentAction = PaymentActionCodeType.SALE;
                    var wrapper = new DoExpressCheckoutPaymentReq { DoExpressCheckoutPaymentRequest = ecRequest };
                    var doEcResponse = service.DoExpressCheckoutPayment(wrapper);

                    if (doEcResponse.Ack == AckCodeType.SUCCESS)
                    {
                        result.IsSuccess = true;
                        result.TransactionId = doEcResponse.DoExpressCheckoutPaymentResponseDetails.PaymentInfo[0].TransactionID;
                    }
                    else
                    {
                        foreach (var error in doEcResponse.Errors)
                        {
                            result.ErrorMessage +=
                                $"[{error.SeverityCode}] {error.ErrorCode} : {error.LongMessage} </br>";
                        }
                    }
                }
                catch
                {
                    result.ErrorMessage = _localizationService.GetResource("Plugin.PaymentMethod.PayPal.ServiceUnavailable");
                }
            });

            return result;
        }

        private Dictionary<string, string> GetPayPalConfig()
        {
            var settings = _settingService.LoadSetting<PayPalSettingsModel>();
            var configMap = new Dictionary<string, string>
            {
                {"account1.apiUsername", settings.APIUserName},
                {"account1.apiPassword", settings.APIPassword},
                {"account1.apiSignature", settings.APISignature},
                // sandbox OR live
                {"mode", settings.RequestMode},
                // https://api-3t.sandbox.paypal.com/nvp
                {"endpoint", settings.EndpointUrl},
                // 20000
                {"connectionTimeout", settings.ConnectionTimeout.ToString()},
                // 4
                {"requestRetries", settings.RequestRetries.ToString()}
            };

            return configMap;
        }

        private string AppendQueryStringToUrl(string url, string parameter, string value)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                if (string.IsNullOrWhiteSpace(uri.Query))
                {
                    return url.TrimEnd('/') + "?" + parameter + "=" + HttpUtility.UrlEncode(value);
                }

                return url + "&" + parameter + "=" + HttpUtility.UrlEncode(value);
            }
            return "";
        }

        public string PaymentGatewayName => _localizationService.GetResource("Plugin.PaymentMethod.PayPal.PaymentGatewayName");
        public string PaymentGatewaySystemName => "paypal";
        public List<string> AcceptedCurrenciesIso => new List<string>() { "usd" /*US Dollar*/, "eur" /*Euro*/};
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
