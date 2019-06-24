using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostDescriptionModel
    {
        public int? Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("Title")]
        public LocalizedString Title { get; set; }


        [RequiredLocalized()]
        [DisplayNameLocalized("Description")]
        [AllowHtml]
        public LocalizedString HtmlDescription { get; set; }


        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }

        [DisplayNameLocalized("AddToSearchEngineIndexes")]
        public bool AddToSearchEngineIndexes { get; set; } = true;
    }
}