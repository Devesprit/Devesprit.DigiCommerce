using Devesprit.Core.Settings;
using Devesprit.WebFramework.Attributes;

namespace Plugin.PaymentMethod.PayPal.Models
{
    public partial class PayPalSettingsModel : ISettings
    {
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.APIUserName")]
        public string APIUserName { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.APIPassword")]
        public string APIPassword { get; set; }
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.APISignature")]
        public string APISignature { get; set; }

        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.PayPalHeaderImageUrl")]
        public string PayPalHeaderImageUrl { get; set; }
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.PayPalBrandName")]
        public string PayPalBrandName { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.PaymentPageUrl")]
        public string PaymentPageUrl { get; set; } = "https://www.sandbox.paypal.com/us/cgi-bin/webscr?cmd=";

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.EndpointUrl")]
        public string EndpointUrl { get; set; } = "https://api-3t.sandbox.paypal.com/nvp";
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.RequestMode")]
        public string RequestMode { get; set; } = "sandbox";
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.ConnectionTimeout")]
        public int ConnectionTimeout { get; set; } = 20000;
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.PayPal.RequestRetries")]
        public int RequestRetries { get; set; } = 4;
    }
}
