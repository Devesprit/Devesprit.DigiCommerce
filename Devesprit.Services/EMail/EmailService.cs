using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Services.Events;
using Devesprit.Services.TemplateEngine;
using Hangfire;
using Microsoft.AspNet.Identity;

namespace Devesprit.Services.EMail
{
    public partial class EmailService: IEmailService
    {
        private readonly ISettingService _settingService;
        private readonly ITemplateEngine _templateEngine;
        private readonly IWorkContext _workContext;
        private readonly IEventPublisher _eventPublisher;

        public EmailService(ISettingService settingService, 
            ITemplateEngine templateEngine, 
            IWorkContext workContext,
            IEventPublisher eventPublisher)
        {
            _settingService = settingService;
            _templateEngine = templateEngine;
            _workContext = workContext;
            _eventPublisher = eventPublisher;
        }
         
        public virtual async Task SendAsync(IdentityMessage message)
        {
            await SendEmailAsync(message.Body, message.Subject, message.Destination);
        }

        public virtual async Task SendEmailAsync(string body, string subject, string destination, string from = null)
        {
            var settings = await _settingService.LoadSettingAsync<SiteSettings>();
            BackgroundJob.Enqueue(() => EmailSender.SendMail(settings.SMTPServer, settings.SMTPPort,
                settings.SMTPEnableSsl,
                settings.SMTPUserName, settings.SMTPPassword,
                settings.SiteEmailAddress,
                settings.SiteName[0], destination,
                subject, body, from));

            _eventPublisher.Publish(new SendEmailEvent(subject, body, destination));
        }
        
        public virtual async Task SendEmailFromTemplateAsync(string templateFileName, string subject, string destination, object model, string from = null)
        {
            var serverRoot = HttpContext.Current.Server.MapPath("~").TrimEnd('\\') + "\\EmailTemplates\\";
            var currentLangIso = _workContext.CurrentLanguage;
            string templateFile;
            if (File.Exists(serverRoot + templateFileName + "." + currentLangIso.IsoCode + ".html"))
            {
                templateFile = serverRoot + templateFileName + "." + currentLangIso.IsoCode + ".html";
            }
            else
            if (File.Exists(serverRoot + templateFileName + ".html"))
            {
                templateFile = serverRoot + templateFileName + ".html";
            }
            else
            {
                throw new FileNotFoundException("Template file not found!\n\n" + serverRoot + templateFileName + "." +
                                                currentLangIso.IsoCode + ".html\n" + serverRoot + templateFileName +
                                                ".html");
            }

            await SendEmailAsync(_templateEngine.CompileTemplateFromFile(templateFile, model), subject, destination, from);
        }
    }

    internal static class EmailSender
    {
        [AutomaticRetry(Attempts = 10)]
        public static void SendMail(string smtpServer, int smtpPort, bool smtpEnableSsl, string smtpUserName, string smtpPassword, string sender, string senderName, string recipient, string subject, string messageBody, string replyTo = null)
        {
            using (var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = smtpEnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUserName, smtpPassword)
            })
            {
                var from = new MailAddress(sender, senderName, System.Text.Encoding.UTF8);
                var to = new MailAddress(recipient);

                var mailMessage = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = messageBody,
                    SubjectEncoding = System.Text.Encoding.UTF8,
                    BodyEncoding = System.Text.Encoding.UTF8,
                    IsBodyHtml = true,
                };
                if (!string.IsNullOrWhiteSpace(replyTo))
                {
                    mailMessage.ReplyToList.Add(new MailAddress(replyTo));
                }

                smtpClient.Send(mailMessage);
            }
        }
    }
}
