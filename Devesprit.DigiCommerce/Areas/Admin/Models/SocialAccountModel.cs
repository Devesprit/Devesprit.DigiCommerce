using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class SocialAccountModel
    {
        public int? Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(500)]
        [DisplayNameLocalized("SocialNetworkName")]
        public LocalizedString SocialNetworkName { get; set; } = new LocalizedString();

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("SocialNetworkIconUrl")]
        public LocalizedString SocialNetworkIconUrl { get; set; } = new LocalizedString();

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("SocialNetworkLargeIconUrl")]
        public LocalizedString SocialNetworkLargeIconUrl { get; set; } = new LocalizedString();

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(1000)]
        [DisplayNameLocalized("YourAccountUrl")]
        public LocalizedString YourAccountUrl { get; set; } = new LocalizedString();
    }
}