
using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class PostTagModel
    {
        public int? Id { get; set; }


        [RequiredLocalized()]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("Tag")]
        public LocalizedString Tag { get; set; }
    }
}