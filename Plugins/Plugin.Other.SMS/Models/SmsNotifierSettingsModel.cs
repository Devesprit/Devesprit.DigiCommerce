using System.ComponentModel;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.WebFramework.Attributes;

namespace Plugin.Other.SMS.Models
{
    public enum SmsProviders{
        [Description("Plugin.Other.SMS.Nexmo")]
        Nexmo,
        [Description("Plugin.Other.SMS.Twilio")]
        Twilio,
        [Description("Plugin.Other.SMS.KaveNeghar")]
        KaveNeghar
    }

    public partial class SmsNotifierSettingsModel : ISettings
    {
        [DisplayNameLocalized("Plugin.Other.SMS.ActiveSmsProvider")]
        public SmsProviders? ActiveSmsProvider { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.KaveNegharWebServiceUrl")]
        public string KaveNegharWebServiceUrl { get; set; } = "http://api.kavenegar.com/soap/v1.asmx?WSDL";

        [DisplayNameLocalized("Plugin.Other.SMS.KaveNegharAPIKey")]
        public string KaveNegharAPIKey { get; set; }
        
        [DisplayNameLocalized("Plugin.Other.SMS.NexmoAPIKey")]
        public string NexmoAPIKey { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.NexmoAPISecret")]
        public string NexmoAPISecret { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.TwilioSID")]
        public string TwilioSID { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.TwilioToken")]
        public string TwilioToken { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut")]
        public bool SendSMSToAdminOnInvoiceCheckOut { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined")]
        public bool SendSMSToAdminOnNewUserJoined { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut")]
        public LocalizedString SMSMessageForInvoiceCheckOut { get; set; } = new LocalizedString("Invoice Checkout - User: {{Invoice.User.Email}} - Amount: {{PaidAmount}}");

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForNewUserJoined")]
        public LocalizedString SMSMessageForNewUserJoined { get; set; } = new LocalizedString("New User Registered - {{User.Email}}");

        [DisplayNameLocalized("Plugin.Other.SMS.AdminMobileNumbers")]
        public string AdminMobileNumbers { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendFromNumber")]
        public string SendFromNumber { get; set; }
    }
}
