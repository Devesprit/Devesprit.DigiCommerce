using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_LocalizedStrings")]
    public partial class TblLocalizedStrings:BaseEntity
    {
        [Required]
        public int LanguageId { get; set; }
        public virtual TblLanguages Language { get; set; }
        [Required]
        public string ResourceName { get; set; }
        [Required]
        public string ResourceValue { get; set; }
    }
}
