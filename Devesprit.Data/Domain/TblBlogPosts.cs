using System.ComponentModel.DataAnnotations.Schema;

namespace Devesprit.Data.Domain
{
    [Table("Tbl_BlogPosts")]
    public partial class TblBlogPosts : TblPosts
    {
        public TblBlogPosts()
        {
            PostType = Enums.PostType.BlogPost;
        }
    }
}
