using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_LocalizedStrings")]
    public partial class TblLocalizedStrings:BaseEntity
    {
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int LanguageId { get; set; }
        public virtual TblLanguages Language { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index(IsClustered = false, IsUnique = false)]
        public string ResourceName { get; set; }
        [Required]
        public string ResourceValue { get; set; }
    }
}
