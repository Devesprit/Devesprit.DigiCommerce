using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_PostTags")]
    public partial class TblPostTags : BaseEntity
    {
        [Required,
         StringLength(400),
         Index(IsUnique = true),
         Column(TypeName = "NVARCHAR")]
        public string Tag { get; set; }
        public virtual ICollection<TblPosts> Posts { get; set; }
    }
}
