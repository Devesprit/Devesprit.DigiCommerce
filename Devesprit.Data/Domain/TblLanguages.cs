using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Languages")]
    public partial class TblLanguages: BaseEntity
    {
        [Required, MaxLength(100)]
        public string LanguageName { get; set; }
        [Required, MaxLength(10)]
        public string IsoCode { get; set; }
        public bool IsDefault { get; set; }
        public bool IsRtl { get; set; }
        public byte[] Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public int? DefaultCurrencyId { get; set; }
        public virtual TblCurrencies DefaultCurrency { get; set; }
        public virtual ICollection<TblLocalizedStrings> LocalizedStrings { get; set; }

    }
}
