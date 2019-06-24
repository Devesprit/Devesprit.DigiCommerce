using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class ReplyToUserMessageModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime ReceiveDate { get; set; }
        public DateTime? ReplyDate { get; set; }
        public string Message { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(250)]
        [EmailAddressLocalized()]
        [DisplayNameLocalized("Email")]
        public string Email { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("Subject")]
        public string Subject { get; set; }
        
        [DisplayNameLocalized("Response")]
        [AllowHtml]
        public string ResponseText { get; set; }
    }
}