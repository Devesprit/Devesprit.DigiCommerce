using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class TaxModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(250)]
        [DisplayNameLocalized("Name")]
        public LocalizedString TaxName { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("TaxPercentage")]
        public double Amount { get; set; }

        [DisplayNameLocalized("IsActive")]
        public bool IsActive { get; set; }
    }
}