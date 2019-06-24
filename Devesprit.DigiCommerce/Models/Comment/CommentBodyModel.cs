using System;

namespace Devesprit.DigiCommerce.Models.Comment
{
    public partial class CommentBodyModel
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public string Quote { get; set; }
        public DateTime ParentCommentDate { get; set; }
        public string ParentCommentUserName { get; set; }
        public int ParentCommentId { get; set; }
        public DateTime CommentDate { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public int PostId { get; set; }
        public bool Published { get; set; }
        public bool CurrentUserIsAdmin { get; set; }
    }
}