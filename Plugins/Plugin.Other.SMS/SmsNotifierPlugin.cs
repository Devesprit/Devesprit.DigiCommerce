using System.Web.Routing;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Services.Localization;
using Plugin.Other.SMS.Models;

namespace Plugin.Other.SMS
{
    public partial class SmsNotifierPlugin : BasePlugin
    {
        private readonly ISettingService _settingService;

        public SmsNotifierPlugin(ISettingService settingsService)
        {
            _settingService = settingsService;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "SmsNotifier";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.Other.SMS.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            _settingService.SaveSetting(new SmsNotifierSettingsModel());

            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Configuration", "SMS Notifier Plugin Configurations", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNeghar", "Kave Neghar (https://www.kavenegar.com)", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Nexmo", "Nexmo (https://www.nexmo.com)", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Twilio", "Twilio (https://www.twilio.com)", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.ActiveSmsProvider", "Active SMS Provider Company", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharWebServiceUrl", "Kave Neghar WebService Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharAPIKey", "Kave Neghar API Key", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendFromNumber", "Send from Number", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoUrl", "Nexmo Service Url", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey", "Nexmo API Key", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret", "Nexmo API Secret", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioSID", "Twilio SID", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioToken", "Twilio Token", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut", "Send SMS on invoice checkout", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined", "Send SMS on new user joined", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewComment", "Send SMS on new comment received", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewMessage", "Send SMS on new 'Contact Us' message received", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnSearchIndexesFailed", "Send SMS on 'Creating Search Engine Indexes Failed'", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut", "Message Template for Invoice Checkout<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined", "Message Template for New User Joined<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewComment", "Message Template for New Comment<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewMessage", "Message Template for New Message<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForSearchIndexesFailed", "Message Template for 'Creating Search Engine Indexes Failed'<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.InvoiceCheckOutRecipients", "'Invoice CheckOut' Message Recipients Phone Number<br/><small class='text-muted'>(Separate recipients with a comma)</small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewUserJoinedRecipients", "'New User Joined' Message Recipients Phone Number<br/><small class='text-muted'>(Separate recipients with a comma)</small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewCommentRecipients", "'New Comment' Message Recipients Phone Number<br/><small class='text-muted'>(Separate recipients with a comma)</small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewMessageRecipients", "'New Contact Us Message' Message Recipients Phone Number<br/><small class='text-muted'>(Separate recipients with a comma)</small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SearchIndexesFailedRecipients", "'Creating Search Engine Indexes Failed' Message Recipients Phone Number<br/><small class='text-muted'>(Separate recipients with a comma)</small>", "en");

            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Configuration", "تنظیمات پیام کوتاه", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNeghar", "کاوه نگار (https://www.kavenegar.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Nexmo", "Nexmo (https://www.nexmo.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Twilio", "Twilio (https://www.twilio.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.ActiveSmsProvider", "تامین کننده خدمات پیام کوتاه فعال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharWebServiceUrl", "آدرس URL وب سرویس کاوه نگار", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharAPIKey", "API Key کاوه نگار", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendFromNumber", "ارسال از شماره", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoUrl", "Nexmo Service Url", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey", "Nexmo API Key", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret", "Nexmo API Secret", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioSID", "Twilio SID", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioToken", "Twilio Token", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut", "ارسال پیام هنگام پرداخت وجه فاکتور", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined", "ارسال پیام هنگام ثبت نام کاربر جدید", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewComment", "ارسال پیام هنگام ثبت کامنت جدید", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewMessage", "ارسال پیام هنگام دریافت پیغام جدید", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnSearchIndexesFailed", "ارسال پیام هنگام بروز خطا در ایجاد شاخص های موتور جستجو", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut", "متن پیام هنگام پرداخت وجه فاکتور<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined", "متن پیام هنگام ثبت نام کاربر جدید<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewComment", "متن پیام هنگام ثبت کامنت جدید<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewMessage", "متن پیام هنگام دریافت پیغام جدید<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForSearchIndexesFailed", "متن پیام هنگام بروز خطا در ایجاد شاخص های موتور جستجو<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.InvoiceCheckOutRecipients", "شماره همراه گیرندگان پیام 'پرداخت فاکتور'<br/><small class='text-muted'>(با کاما ',' جدا کنید)</small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewUserJoinedRecipients", "شماره همراه گیرندگان پیام 'ثبت نام کاربر جدید'<br/><small class='text-muted'>(با کاما ',' جدا کنید)</small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewCommentRecipients", "شماره همراه گیرندگان پیام 'دریافت کامنت جدید'<br/><small class='text-muted'>(با کاما ',' جدا کنید)</small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NewMessageRecipients", "شماره همراه گیرندگان پیام 'دریافت پیغام جدید'<br/><small class='text-muted'>(با کاما ',' جدا کنید)</small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SearchIndexesFailedRecipients", "شماره همراه گیرندگان پیام 'بروز خطا در ایجاد شاخص های موتور جستجو'<br/><small class='text-muted'>(با کاما ',' جدا کنید)</small>", "fa");

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<SmsNotifierSettingsModel>();

            this.DeletePluginLocaleResource("Plugin.Other.SMS.Configuration");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.KaveNeghar");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.Nexmo");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.Twilio");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.ActiveSmsProvider");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.KaveNegharWebServiceUrl");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.KaveNegharAPIKey");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendFromNumber");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NexmoUrl");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.TwilioSID");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.TwilioToken");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewComment");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewMessage");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnSearchIndexesFailed");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewComment");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewMessage");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForSearchIndexesFailed");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.InvoiceCheckOutRecipients");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NewUserJoinedRecipients");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NewCommentRecipients");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NewMessageRecipients");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SearchIndexesFailedRecipients");

            base.Uninstall();
        }
    }
}
