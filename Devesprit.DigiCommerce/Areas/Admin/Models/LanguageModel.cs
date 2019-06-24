using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.WebFramework.Attributes;


namespace Devesprit.DigiCommerce.Areas.Admin.Models
{
    public partial class LanguageModel
    {
        public int? Id { get; set; }

        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(100)]
        [DisplayNameLocalized("LanguageLocalName")]
        public string LanguageName { get; set; }


        [RequiredLocalized(AllowEmptyStrings = false)]
        [MaxLengthLocalized(2)]
        [DisplayNameLocalized("IsoCode")]
        public string IsoCode { get; set; }

        [DisplayNameLocalized("Rtl")]
        public bool IsRtl { get; set; }

        [DisplayNameLocalized("Icon")]
        [FileExtensions("png,jpg,jpeg", "SelectImageFile")]
        [MaxFileSize(50 * 1024)]
        public HttpPostedFileBase Icon { get; set; }

        [DisplayNameLocalized("DisplayOrder")]
        [RequiredLocalized(AllowEmptyStrings = false)]
        public int DisplayOrder { get; set; }

        [DisplayNameLocalized("Published")]
        public bool Published { get; set; } = true;

        [RequiredLocalized(AllowEmptyStrings = false)]
        [DisplayNameLocalized("DefaultCurrency")]
        public int DefaultCurrencyId { get; set; }

        public List<SelectListItem> CurrenciesList { get; set; }

        public bool IsDefault { get; set; }
    }
}