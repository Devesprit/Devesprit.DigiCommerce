using System;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Events;
using Devesprit.Services.Events;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.TemplateEngine;
using Devesprit.Services.Users.Events;
using Nexmo.Api;
using Plugin.Other.SMS.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Plugin.Other.SMS
{
    public class InvoiceCheckoutEventHandler : IConsumer<InvoiceCheckoutEvent>, IConsumer<UserSignupEvent>
    {
        private readonly ISettingService _settingService;
        private readonly ITemplateEngine _templateEngine;

        public InvoiceCheckoutEventHandler(ISettingService settingService, ITemplateEngine templateEngine)
        {
            _settingService = settingService;
            _templateEngine = templateEngine;
        }

        public void HandleEvent(InvoiceCheckoutEvent eventMessage)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.AdminMobileNumbers.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnInvoiceCheckOut && settings.ActiveSmsProvider != null && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForInvoiceCheckOut),
                        eventMessage);

                Task.Run(() =>
                {
                    switch (settings.ActiveSmsProvider)
                    {
                        case SmsProviders.KaveNeghar:
                            var statusCode = 0;
                            var statusMessage = "";
                            using (var smsService = new com.kavenegar.api.v1(){Url = settings.KaveNegharWebServiceUrl})
                            {
                                smsService.SendSimpleByApikey(settings.KaveNegharAPIKey,
                                    settings.SendFromNumber,
                                    messageText, 
                                    recipients.ToArray(), 0, 0,
                                    ref statusCode, ref statusMessage);
                            }
                            break;
                        case SmsProviders.Nexmo:
                            var client = new Client(creds: new Nexmo.Api.Request.Credentials
                            {
                                ApiKey = settings.NexmoAPIKey,
                                ApiSecret = settings.NexmoAPISecret
                            });

                            foreach (var recipient in recipients)
                            {
                                client.SMS.Send(new Nexmo.Api.SMS.SMSRequest()
                                {
                                    from = settings.SendFromNumber,
                                    to = recipient,
                                    text = messageText
                                });
                            }
                            break;
                        case SmsProviders.Twilio:
                            TwilioClient.Init(settings.TwilioSID, settings.TwilioToken);

                            foreach (var recipient in recipients)
                            {
                                MessageResource.Create(
                                    body: messageText,
                                    from: new Twilio.Types.PhoneNumber(settings.SendFromNumber),
                                    to: new Twilio.Types.PhoneNumber(recipient)
                                );
                            }
                            break;
                    }
                });
            }
        }

        public void HandleEvent(UserSignupEvent eventMessage)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.AdminMobileNumbers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnNewUserJoined && settings.ActiveSmsProvider != null && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForNewUserJoined),
                        eventMessage);

                Task.Run(() =>
                {
                    switch (settings.ActiveSmsProvider)
                    {
                        case SmsProviders.KaveNeghar:
                            var statusCode = 0;
                            var statusMessage = "";
                            using (var smsService = new com.kavenegar.api.v1() { Url = settings.KaveNegharWebServiceUrl })
                            {
                                smsService.SendSimpleByApikey(settings.KaveNegharAPIKey,
                                    settings.SendFromNumber,
                                    messageText,
                                    recipients.ToArray(), 0, 0,
                                    ref statusCode, ref statusMessage);
                            }
                            break;
                        case SmsProviders.Nexmo:
                            var client = new Client(creds: new Nexmo.Api.Request.Credentials
                            {
                                ApiKey = settings.NexmoAPIKey,
                                ApiSecret = settings.NexmoAPISecret
                            });

                            foreach (var recipient in recipients)
                            {
                                client.SMS.Send(new Nexmo.Api.SMS.SMSRequest()
                                {
                                    from = settings.SendFromNumber,
                                    to = recipient,
                                    text = messageText
                                });
                            }
                            break;
                        case SmsProviders.Twilio:
                            TwilioClient.Init(settings.TwilioSID, settings.TwilioToken);

                            foreach (var recipient in recipients)
                            {
                                MessageResource.Create(
                                    body: messageText,
                                    from: new Twilio.Types.PhoneNumber(settings.SendFromNumber),
                                    to: new Twilio.Types.PhoneNumber(recipient)
                                );
                            }
                            break;
                    }
                });
            }
        }
    }
}