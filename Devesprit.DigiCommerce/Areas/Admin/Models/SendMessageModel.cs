using System.Web.Mvc;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class SendMessageModel
    {
        [RequiredLocalized()]
        [DisplayNameLocalized("Recipient")]
        public string Recipient { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("Message")]
        [AllowHtml]
        public string Message { get; set; }
    }
}