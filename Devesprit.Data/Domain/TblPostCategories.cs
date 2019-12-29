using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Devesprit.Data.Enums;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_PostCategories")]
    public partial class TblPostCategories: BaseEntity
    {
        [Required, MaxLength(500)]
        public string CategoryName { get; set; }
        [Required,
         StringLength(500),
         Index(IsUnique = true),
        Column(TypeName = "NVARCHAR")]
        public string Slug { get; set; }
        public bool ShowInFooter { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
        public DisplayArea DisplayArea { get; set; }
        public virtual TblPostCategories ParentCategory { get; set; }
        public virtual ICollection<TblPosts> Posts { get; set; } = new HashSet<TblPosts>();
        public virtual ICollection<TblPostCategories> SubCategories { get; set; }
    }
}
