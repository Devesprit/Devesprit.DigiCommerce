using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Devesprit.Services.EMail
{
    public partial interface IEmailService: IIdentityMessageService
    {
        Task SendEmailAsync(string body, string subject, string destination, string from = null);
        Task SendEmailFromTemplateAsync(string templateFileName, string subject, string destination, object model, string from = null);
    }
}
