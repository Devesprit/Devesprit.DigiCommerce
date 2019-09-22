using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_LocalizedProperty")]
    public partial class TblLocalizedProperty:BaseEntity
    {
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int EntityId { get; set; }
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int LanguageId { get; set; }
        public virtual TblLanguages Language { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index(IsClustered = false, IsUnique = false)]
        public string LocaleKeyGroup { get; set; }
        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index(IsClustered = false, IsUnique = false)]
        public string LocaleKey { get; set; }
        [Required]
        public string LocaleValue { get; set; }
    }
}
