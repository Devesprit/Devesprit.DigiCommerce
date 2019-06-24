using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostAttributeModel
    {
        public int? Id { get; set; }


        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("AttributeName")]
        [AllowHtml]
        public LocalizedString Name { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("AttributeType")]
        public PostAttributeType AttributeType { get; set; }
    }
}