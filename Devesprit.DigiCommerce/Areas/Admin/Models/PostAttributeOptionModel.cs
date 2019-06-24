using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostAttributeOptionModel
    {
        public int? Id { get; set; }


        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("OptionName")]
        [AllowHtml]
        public LocalizedString Name { get; set; }

        public int PostAttributeId { get; set; }
    }
}