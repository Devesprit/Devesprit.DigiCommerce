using Devesprit.Core.Settings;
using Devesprit.WebFramework.Attributes;

namespace Plugin.PaymentMethod.Zarinpal.Models
{
    public partial class ZarinPalSettingsModel : ISettings
    {
        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.Zarinpal.MerchantID")]
        public string MerchantId { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.Zarinpal.ZarinpalWebserviceUrl")]
        public string ZarinpalWebserviceUrl { get; set; } = "https://www.zarinpal.com/pg/services/WebGate/service";

        [RequiredLocalized()]
        [DisplayNameLocalized("Plugin.PaymentMethod.Zarinpal.ZarinpalPaymentPageUrl")]
        public string ZarinpalPaymentPageUrl { get; set; } = "https://www.zarinpal.com/pg/StartPay/{0}";
    }
}
