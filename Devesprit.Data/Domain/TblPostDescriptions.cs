using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_PostDescriptions")]
    public partial class TblPostDescriptions: BaseEntity
    {
        [Required]
        public int PostId { get; set; }
        public virtual TblPosts Post { get; set; }
        [Required, MaxLength(250)]
        public string Title { get; set; }
        [Required]
        public string HtmlDescription { get; set; }
        public int DisplayOrder { get; set; }
        public bool AddToSearchEngineIndexes { get; set; }
    }
}
