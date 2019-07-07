using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Data.Events;
using Devesprit.Services.Events;
using Devesprit.Services.Invoice;
using Devesprit.Services.Localization;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.TemplateEngine;
using Devesprit.Services.Users;
using Devesprit.Services.Users.Events;
using Plugin.Other.SMS.Models;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Plugin.Other.SMS
{
    public class InvoiceCheckoutEventHandler : IConsumer<InvoiceCheckoutEvent>, IConsumer<UserSignupEvent>,
        IConsumer<EntityInserted<TblPostComments>>, IConsumer<EntityInserted<TblUserMessages>>,
        IConsumer<CreateSearchIndexesFailEvent>
    {
        private readonly ISettingService _settingService;
        private readonly ITemplateEngine _templateEngine;
        private readonly IUsersService _usersService;

        public InvoiceCheckoutEventHandler(ISettingService settingService, ITemplateEngine templateEngine, IUsersService usersService)
        {
            _settingService = settingService;
            _templateEngine = templateEngine;
            _usersService = usersService;
        }

        public void HandleEvent(InvoiceCheckoutEvent eventMessage)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.InvoiceCheckOutRecipients.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnInvoiceCheckOut && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForInvoiceCheckOut),
                        eventMessage);

                SendMessage(messageText, recipients);
            }
        }

        public void HandleEvent(UserSignupEvent eventMessage)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.NewUserJoinedRecipients.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnNewUserJoined && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForNewUserJoined),
                        eventMessage);

                SendMessage(messageText, recipients);
            }
        }

        public void HandleEvent(EntityInserted<TblPostComments> eventMessage)
        {
            if (!string.IsNullOrWhiteSpace(eventMessage.Entity.UserId) && _usersService.UserIsAdmin(eventMessage.Entity.UserId))
            {
                return;
            }

            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.NewCommentRecipients.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnNewComment && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForNewComment),
                        eventMessage);

                SendMessage(messageText, recipients);
            }
        }

        public void HandleEvent(EntityInserted<TblUserMessages> eventMessage)
        {
            if (!string.IsNullOrWhiteSpace(eventMessage.Entity.UserId) && _usersService.UserIsAdmin(eventMessage.Entity.UserId))
            {
                return;
            }

            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.NewMessageRecipients.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnNewMessage && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForNewMessage),
                        eventMessage);

                SendMessage(messageText, recipients);
            }
        }

        public void HandleEvent(CreateSearchIndexesFailEvent eventMessage)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            var recipients = settings.SearchIndexesFailedRecipients.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
            if (settings.SendSMSToAdminOnSearchIndexesFailed && recipients.Any())
            {
                var messageText =
                    _templateEngine.CompileTemplate(settings.GetLocalized(x => x.SMSMessageForSearchIndexesFailed),
                        eventMessage);

                SendMessage(messageText, recipients);
            }
        }

        private void SendMessage(string messageText, List<string> recipients)
        {
            var settings = _settingService.LoadSetting<SmsNotifierSettingsModel>();

            if (settings.ActiveSmsProvider != null && !string.IsNullOrWhiteSpace(settings.SendFromNumber))
            {
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
                            foreach (var recipient in recipients)
                            {
                                try
                                {
                                    using (var webClient = new WebClient())
                                    {
                                        webClient.Encoding = Encoding.UTF8;
                                        string result = webClient.DownloadString(string.Format(settings.NexmoUrl,
                                            settings.SendFromNumber, recipient, settings.NexmoAPIKey,
                                            settings.NexmoAPISecret, messageText));
                                    }
                                }
                                catch{}
                            }
                            break;
                        case SmsProviders.Twilio:
                            TwilioClient.Init(settings.TwilioSID, settings.TwilioToken);

                            try
                            {
                                foreach (var recipient in recipients)
                                {
                                    MessageResource.Create(
                                        body: messageText,
                                        from: new Twilio.Types.PhoneNumber(settings.SendFromNumber),
                                        to: new Twilio.Types.PhoneNumber(recipient)
                                    );
                                }
                            }
                            catch
                            {}
                            break;
                    }
                });
            }
        }
    }
}