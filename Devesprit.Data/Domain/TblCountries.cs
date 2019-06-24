using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_Countries")]
    public partial class TblCountries:BaseEntity
    {
        [Required, MaxLength(250)]
        public string CountryName { get; set; }
    }
}
