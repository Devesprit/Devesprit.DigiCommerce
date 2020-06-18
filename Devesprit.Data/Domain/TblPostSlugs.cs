using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table(("Tbl_PostSlugs"))]
    public partial class TblPostSlugs : BaseEntity
    {
        [Required]
        [Index(IsClustered = false, IsUnique = false)]
        public int PostId { get; set; }
        public virtual TblPosts Post { get; set; }
        [Required,
         StringLength(500),
         Index(IsClustered = false, IsUnique = true),
         Column(TypeName = "VARCHAR")]
        public string Slug { get; set; }
    }
}