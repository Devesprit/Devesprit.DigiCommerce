using Devesprit.Data.Events;

namespace Devesprit.Services.EMail
{
    public partial class SendEmailEvent : IEvent
    {
        public SendEmailEvent(string subject, string message, string recipient)
        {
            this.Subject = subject;
            this.Message = message;
            this.Recipient = recipient;
        }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Recipient { get; set; }
    }
}
