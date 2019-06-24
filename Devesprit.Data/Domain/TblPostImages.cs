using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table(("Tbl_PostImages"))]
    public partial class TblPostImages : BaseEntity
    {
        [Required]
        public int PostId { get; set; }
        public virtual TblPosts Post { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        public string Alt { get; set; }
        public string Title { get; set; }
        public int DisplayOrder { get; set; }
    }
}
