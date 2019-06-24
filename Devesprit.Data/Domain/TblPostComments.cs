using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_PostComments")]
    public partial class TblPostComments : BaseEntity
    {
        [Required]
        public string Comment { get; set; }
        public string Quote { get; set; }
        [Required]
        public DateTime CommentDate { get; set; }
        [Required]
        public int PostId { get; set; }
        public virtual TblPosts Post { get; set; }
        public int? ParentCommentId { get; set; }
        public virtual TblPostComments ParentComment { get; set; }
        public string UserId { get; set; }
        public virtual TblUsers User { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public bool Published { get; set; }
        public bool NotifyWhenReply { get; set; }
        public bool NotifyWhenNewComment { get; set; }
    }
}
