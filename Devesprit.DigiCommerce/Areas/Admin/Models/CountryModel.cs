using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;

namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class CountryModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("CountryName")]
        public LocalizedString CountryName { get; set; } = new LocalizedString();
    }
}