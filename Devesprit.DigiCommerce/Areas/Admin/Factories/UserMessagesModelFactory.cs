using System.Web;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Utilities.Extensions;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class UserMessagesModelFactory : IUserMessagesModelFactory
    {
        public virtual ReplyToUserMessageModel PrepareReplyToUserMessageModel(TblUserMessages message)
        {
            var result = message == null ? new ReplyToUserMessageModel() : message.Adapt<ReplyToUserMessageModel>();

            var userMessage = HttpUtility.HtmlEncode(result.Message);
            userMessage = userMessage.Replace("\r\n", "\r");
            userMessage = userMessage.Replace("\n", "\r");
            userMessage = userMessage.Replace("\r", "<br/>\r\n");
            userMessage = userMessage.Replace("  ", " &nbsp;");
            result.Message = userMessage;

            if (string.IsNullOrWhiteSpace(result.ResponseText))
            {
                result.ResponseText =
                    $"<div style='direction: {(result.Message.IsRtlLanguage() ? "rtl" : "ltr")}'><p><br/></p><hr/><small><blockquote>{userMessage}</blockquote></small></div>";
            }

            return result;
        }
    }
}