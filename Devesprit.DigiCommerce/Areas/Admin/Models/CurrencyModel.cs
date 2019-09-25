using Devesprit.Core.Localization;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class CurrencyModel
    {
        public int? Id { get; set; }

        [RequiredLocalized()]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("CurrencyName")]
        public LocalizedString CurrencyName { get; set; } = new LocalizedString();

        [RequiredLocalized()]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("CurrencyShortName")]
        public LocalizedString ShortName { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("CurrencyDisplayFormat")]
        public string DisplayFormat { get; set; }

        [DisplayNameLocalized("ExchangeRate")]
        [RequiredLocalized(AllowEmptyStrings = false)]
        public double ExchangeRate { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(10)]
        [DisplayNameLocalized("IsoCode")]
        public string IsoCode { get; set; }
        
        [DisplayNameLocalized("DisplayOrder")]
        [RequiredLocalized(AllowEmptyStrings = false)]
        public int DisplayOrder { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;

        public bool IsMainCurrency { get; set; }

    }
}