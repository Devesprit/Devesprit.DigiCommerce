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
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey", "Nexmo API Key", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret", "Nexmo API Secret", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioSID", "Twilio SID", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioToken", "Twilio Token", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut", "Send SMS on invoice checkout", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined", "Send SMS on new user joined", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut", "Message Template for Invoice Checkout<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info: https://github.com/lunet-io/scriban</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined", "Message Template for New User Joined<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>More Info: https://github.com/lunet-io/scriban</a></small>", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.AdminMobileNumbers", "Recipient Phone Numbers (Separate recipients with a comma)", "en");

            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Configuration", "تنظیمات پیام کوتاه", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNeghar", "کاوه نگار (https://www.kavenegar.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Nexmo", "Nexmo (https://www.nexmo.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.Twilio", "Twilio (https://www.twilio.com)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.ActiveSmsProvider", "تامین کننده خدمات پیام کوتاه فعال", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharWebServiceUrl", "آدرس URL وب سرویس کاوه نگار", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.KaveNegharAPIKey", "API Key کاوه نگار", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendFromNumber", "ارسال از شماره", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey", "Nexmo API Key", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret", "Nexmo API Secret", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioSID", "Twilio SID", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.TwilioToken", "Twilio Token", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut", "ارسال پیام هنگام پرداخت وجه فاکتور", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined", "ارسال پیام هنگام ثبت نام کاربر جدید", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut", "متن پیام هنگام پرداخت وجه فاکتور<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر: https://github.com/lunet-io/scriban</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined", "متن پیام هنگامثبت نام کاربر جدید<br/><small><a href='https://github.com/lunet-io/scriban' target='_blank'>اطلاعات بیشتر: https://github.com/lunet-io/scriban</a></small>", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Other.SMS.AdminMobileNumbers", "شماره همراه گیرندگان (با کاما ',' جدا کنید)", "fa");

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
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NexmoAPIKey");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.NexmoAPISecret");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.TwilioSID");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.TwilioToken");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnInvoiceCheckOut");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SendSMSToAdminOnNewUserJoined");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForInvoiceCheckOut");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.SMSMessageForNewUserJoined");
            this.DeletePluginLocaleResource("Plugin.Other.SMS.AdminMobileNumbers");

            base.Uninstall();
        }
    }
}
