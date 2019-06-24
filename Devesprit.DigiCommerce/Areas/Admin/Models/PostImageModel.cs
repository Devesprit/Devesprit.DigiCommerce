using System.ComponentModel.DataAnnotations;
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostImageModel
    {
        public int? Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [RequiredLocalized()]
        [DisplayNameLocalized("ImageUrl")]
        public LocalizedString ImageUrl { get; set; }


        [DisplayNameLocalized("AltTag")]
        public LocalizedString Alt { get; set; }


        [DisplayNameLocalized("ImageTitle")]
        public LocalizedString Title { get; set; }


        [DisplayNameLocalized("DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}