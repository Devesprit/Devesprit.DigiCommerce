using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Models.Comment;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories.Interfaces
{
    public partial interface ICommentModelFactory
    {
        CommentsListModel PrepareCommentsListModel(IPagedList<TblPostComments> comments, int postId, bool isAdmin);
        CommentEditorModel PrepareCommentEditorModel(int postId, TblUsers currentUser, bool isAdmin);
        CommentBodyModel PrepareCommentBodyModel(TblPostComments comment, bool isAdmin);
        Task<TblPostComments> PrepareTblCommentsAsync(CommentEditorModel model, string currentUserId, bool published);
    }
}