using X.PagedList;

namespace Devesprit.DigiCommerce.Models.Comment
{
    public partial class CommentsListModel
    {
        public IPagedList<CommentBodyModel> CommentsList { get; set; }
        public int PostId { get; set; }
        public int? HighlightComment { get; set; }
    }
}