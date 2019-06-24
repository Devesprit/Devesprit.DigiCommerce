using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_LocalizedProperty")]
    public partial class TblLocalizedProperty:BaseEntity
    {
        [Required]
        public int EntityId { get; set; }
        [Required]
        public int LanguageId { get; set; }
        public virtual TblLanguages Language { get; set; }
        [Required]
        public string LocaleKeyGroup { get; set; }
        [Required]
        public string LocaleKey { get; set; }
        [Required]
        public string LocaleValue { get; set; }
    }
}
