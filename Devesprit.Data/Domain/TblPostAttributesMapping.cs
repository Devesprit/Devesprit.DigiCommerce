using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_PostAttributesMapping")]
    public partial class TblPostAttributesMapping : BaseEntity
    {
        [Required]
        public int PostId { get; set; }

        public virtual TblPosts Post { get; set; }

        [Required]
        public int PostAttributeId { get; set; }

        public virtual TblPostAttributes PostAttribute { get; set; }

        public int? AttributeOptionId { get; set; }

        public virtual TblPostAttributeOptions AttributeOption { get; set; }

        public string Value { get; set; }

        public int DisplayOrder { get; set; }
    }
}