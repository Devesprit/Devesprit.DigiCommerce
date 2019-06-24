using System;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Comment;
using Devesprit.Services.Comments;
using X.PagedList;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class CommentModelFactory : ICommentModelFactory
    {
        private readonly ICommentsService _commentsService;

        public CommentModelFactory(ICommentsService commentsService)
        {
            _commentsService = commentsService;
        }
        
        public virtual CommentsListModel PrepareCommentsListModel(IPagedList<TblPostComments> comments, int postId, bool isAdmin)
        {
            return new CommentsListModel()
            {
                CommentsList = comments.Select(p => PrepareCommentBodyModel(p, isAdmin)),
                PostId = postId
            };
        }

        public virtual CommentEditorModel PrepareCommentEditorModel(int postId, TblUsers currentUser, bool isAdmin)
        {
            return new CommentEditorModel()
                {
                    PostId = postId,
                    UserName = currentUser != null ? currentUser.FirstName + " " + currentUser.LastName : "",
                    UserEmail = currentUser != null ? currentUser.Email : "",
                    GuestUser = currentUser == null,
                    UserIsAdmin = isAdmin,
                    Published = isAdmin
                };
        }

        public virtual CommentBodyModel PrepareCommentBodyModel(TblPostComments comment, bool isAdmin)
        {
            var parentComment = comment.ParentComment;
            return new CommentBodyModel()
            {
                Id = comment.Id,
                CommentDate = comment.CommentDate,
                UserName = comment.User != null ? comment.User.FirstName + " " + comment.User.LastName : comment.UserName,
                Comment = comment.Comment,
                Quote = comment.Quote,
                UserAvatar = comment.User != null ? comment.User.Avatar : "",
                PostId = comment.PostId,
                Published = comment.Published,
                CurrentUserIsAdmin = isAdmin,
                ParentCommentUserName = parentComment?.User != null ? parentComment?.User.FirstName + " " + parentComment?.User.LastName : parentComment?.UserName,
                ParentCommentDate = parentComment?.CommentDate ?? DateTime.MinValue,
                ParentCommentId = parentComment?.Id ?? 0
            };
        }

        public virtual async Task<TblPostComments> PrepareTblCommentsAsync(CommentEditorModel model, string currentUserId, bool published)
        {
            TblPostComments parentComment = null;
            if (model.ReplyToCommentId != null)
            {
                parentComment = await _commentsService.FindByIdAsync(model.ReplyToCommentId.Value);
            }

            var result = new TblPostComments()
            {
                Comment = model.Comment,
                CommentDate = DateTime.Now,
                NotifyWhenNewComment = model.NotifyWhenNewComment,
                NotifyWhenReply = model.NotifyWhenReply,
                PostId = model.PostId,
                UserEmail = model.UserEmail,
                UserName = model.UserName,
                UserId = currentUserId,
                Published = published,
                ParentCommentId = parentComment?.Id,
                Quote = parentComment?.Comment
            };

            return result;
        }
    }
}