using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Currencies")]
    public partial class TblCurrencies: BaseEntity
    {
        [Required, MaxLength(100)]
        public string CurrencyName { get; set; }
        [Required, MaxLength(100)]
        public string ShortName { get; set; }
        [Required, MaxLength(10)]
        [Column(TypeName = "VARCHAR")]
        [StringLength(10)]
        [Index(IsClustered = false, IsUnique = false)]
        public string IsoCode { get; set; }
        public bool IsMainCurrency { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public double ExchangeRate { get; set; }
        public string DisplayFormat { get; set; }
    }
}
