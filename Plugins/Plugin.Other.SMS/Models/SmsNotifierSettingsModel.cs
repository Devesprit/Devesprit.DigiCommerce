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

        [DisplayNameLocalized("Plugin.Other.SMS.NexmoUrl")]
        public string NexmoUrl { get; set; } =
            "https://rest.nexmo.com/sms/json?from={0}&to={1}&api_key={2}&api_secret={3}&text={4}";

        [DisplayNameLocalized("Plugin.Other.SMS.TwilioSID")]
        public string TwilioSID { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.TwilioToken")]
        public string TwilioToken { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut")]
        public bool SendSMSToAdminOnInvoiceCheckOut { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined")]
        public bool SendSMSToAdminOnNewUserJoined { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnNewComment")]
        public bool SendSMSToAdminOnNewComment { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnNewMessage")]
        public bool SendSMSToAdminOnNewMessage { get; set; }
        
        [DisplayNameLocalized("Plugin.Other.SMS.SendSMSToAdminOnSearchIndexesFailed")]
        public bool SendSMSToAdminOnSearchIndexesFailed { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut")]
        public LocalizedString SMSMessageForInvoiceCheckOut { get; set; } = new LocalizedString("Invoice Checkout - User: {{Invoice.User.Email}} - Amount: {{PaidAmountExStr}} {{CurrencyIso}}, Gateway: {{PaymentGatewayName}}");

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForNewUserJoined")]
        public LocalizedString SMSMessageForNewUserJoined { get; set; } = new LocalizedString("New User Registered - {{User.Email}}");

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForNewComment")]
        public LocalizedString SMSMessageForNewComment { get; set; } = new LocalizedString("New Comment - From: {{Entity.UserEmail}}");

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForNewMessage")]
        public LocalizedString SMSMessageForNewMessage { get; set; } = new LocalizedString("New Message - Subject: {{Entity.Subject}} - From: {{Entity.Email}}");

        [DisplayNameLocalized("Plugin.Other.SMS.SMSMessageForSearchIndexesFailed")]
        public LocalizedString SMSMessageForSearchIndexesFailed { get; set; } = new LocalizedString("An error occurred on creating search engine indexes");

        [DisplayNameLocalized("Plugin.Other.SMS.InvoiceCheckOutRecipients")]
        public string InvoiceCheckOutRecipients { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.NewUserJoinedRecipients")]
        public string NewUserJoinedRecipients { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.NewCommentRecipients")]
        public string NewCommentRecipients { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.NewMessageRecipients")]
        public string NewMessageRecipients { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SearchIndexesFailedRecipients")]
        public string SearchIndexesFailedRecipients { get; set; }

        [DisplayNameLocalized("Plugin.Other.SMS.SendFromNumber")]
        public string SendFromNumber { get; set; }
    }
}
