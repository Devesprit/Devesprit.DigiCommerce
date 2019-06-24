using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Notifications;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Devesprit.Services.Users;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Comments
{
    public partial class CommentsService : ICommentsService
    {
        private readonly AppDbContext _dbContext;
        private readonly IWorkContext _workContext;
        private readonly INotificationsService _notificationsService;
        private readonly IPostService<TblPosts> _postService;
        private readonly IUsersService _usersService;
        private readonly HttpContextBase _httpContext;
        private readonly IEventPublisher _eventPublisher;

        public CommentsService(AppDbContext dbContext, 
            IWorkContext workContext,
            INotificationsService notificationsService,
            IPostService<TblPosts> postService,
            IUsersService usersService,
            HttpContextBase httpContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _workContext = workContext;
            _notificationsService = notificationsService;
            _postService = postService;
            _usersService = usersService;
            _httpContext = httpContext;
            _eventPublisher = eventPublisher;
        }
        
        public virtual async Task<IPagedList<TblPostComments>> GetAsPagedListAsync(bool onlyPublished, int? productId, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var query = GetAsQueryable();
            if (productId != null && productId > 0)
            {
                query = query.Where(p => p.PostId == productId);
            }

            if (onlyPublished)
            {
                query = query.Where(p => p.Published);
            }

            var result = new StaticPagedList<TblPostComments>(
                await query
                    .OrderByDescending(p => p.CommentDate)
                    .Include(p => p.User)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.Comment),
                pageIndex,
                pageSize,
                await query
                    .DeferredCount()
                    .FromCacheAsync(QueryCacheTag.Comment));

            return result;
        }

        public virtual async Task<IPagedList<TblPostComments>> GetUserCommentsAsPagedListAsync(string userId, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var query = GetAsQueryable();
            query = query.Where(p => p.UserId == userId);

            var result = new StaticPagedList<TblPostComments>(
                await query
                    .OrderByDescending(p => p.CommentDate)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.Comment),
                pageIndex,
                pageSize,
                await query
                    .DeferredCount()
                    .FromCacheAsync(QueryCacheTag.Comment));

            return result;
        }

        public virtual async Task<IPagedList<TblPostComments>> FindCommentInListAsync(bool onlyPublished, int? productId, int commentId, int pageSize = int.MaxValue)
        {
            var query = GetAsQueryable();
            if (productId != null && productId > 0)
            {
                query = query.Where(p => p.PostId == productId);
            }

            if (onlyPublished)
            {
                query = query.Where(p => p.Published);
            }

            var rowIndex = await query.OrderByDescending(p => p.CommentDate).DeferredCount(p=> p.Id > commentId).FromCacheAsync(QueryCacheTag.Comment);
            int pageIndex = (rowIndex / pageSize) + 1;

            var result = new StaticPagedList<TblPostComments>(
                await query
                    .OrderByDescending(p => p.CommentDate)
                    .Include(p => p.User)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.Comment),
                pageIndex,
                pageSize,
                await query
                    .DeferredCount()
                    .FromCacheAsync(QueryCacheTag.Comment));

            return result;
        }

        public virtual IQueryable<TblPostComments> GetAsQueryable()
        {
            return _dbContext.PostComments.OrderBy(p => p.CommentDate);
        }

        public virtual async Task<TblPostComments> FindByIdAsync(int id)
        {
            var result = await _dbContext.PostComments
                .Include(p => p.ParentComment)
                .Include(p => p.User)
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Comment);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            await _dbContext.PostComments.Where(p => p.ParentCommentId == id)
                .UpdateAsync(p => new TblPostComments() {ParentCommentId = null});
            var record = await FindByIdAsync(id);
            await _dbContext.PostComments.Where(p=> p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Comment);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPostComments record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.PostComments.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Comment);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostComments record, bool sendNotifications)
        {
            _dbContext.PostComments.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Comment);

            _eventPublisher.EntityInserted(record);

            //Send Notifications
            if (sendNotifications)
            {
                var post = await _postService.FindByIdAsync(record.PostId);

                var urlHelper = new UrlHelper(_httpContext.Request.RequestContext);
                var postUrl = post.PostType == PostType.Product
                    ? urlHelper.Action("Index", "Product", new { slug = post.Slug }, _httpContext.Request.Url.Scheme)
                    : urlHelper.Action("Post", "Blog", new { slug = post.Slug }, _httpContext.Request.Url.Scheme);

                string userId = "";
                if (record.ParentCommentId != null)
                {
                    userId = await GetUserToNotifyReplyComment(record.ParentCommentId.Value);
                    if (!string.IsNullOrWhiteSpace(userId) && userId != _workContext.CurrentUser?.Id)
                    {
                        await _notificationsService.SendNotificationAsync(userId,
                            "Notification_NewReplySubmittedToYourComment",
                            new
                            {
                                PostTitle = post.Title,
                                Url = postUrl,
                                Name = record.UserName,
                                Email = record.UserEmail
                            });
                    }
                }

                var users = await GetUsersToNotifyNewComment(record.PostId);
                await _notificationsService.SendMultipleNotificationsAsync(
                    users.Where(p => p != _workContext.CurrentUser?.Id && p != userId).ToList(),
                    "Notification_NewCommentSubmitted",
                    new
                    {
                        PostTitle = post.Title,
                        Url = postUrl,
                        Name = record.UserName,
                        Email = record.UserEmail
                    });

                if (!_workContext.IsAdmin)
                {
                    //Notification to admin
                    await _notificationsService.SendNotificationAsync("Admin",
                        "Notification_NewCommentSubmitted",
                        new
                        {
                            PostTitle = post.Title,
                            Url = postUrl,
                            Name = record.UserName,
                            Email = record.UserEmail
                        });
                }
                //--------------
            }

            return record.Id;
        }

        public virtual async Task<TblPostComments> SetCommentPublished(int commentId, bool published)
        {
            var comment = await FindByIdAsync(commentId);
            comment.Published = published;
            await UpdateAsync(comment);

            if (published && !string.IsNullOrWhiteSpace(comment.UserId)) //send a notification to user
            {
                if (!_usersService.UserIsAdmin(comment.UserId))
                {
                    var urlHelper = new UrlHelper(_httpContext.Request.RequestContext);

                    var postUrl = comment.Post.PostType == PostType.Product
                        ? urlHelper.Action("Index", "Product", new { slug = comment.Post.Slug }, _httpContext.Request.Url.Scheme)
                        : urlHelper.Action("Post", "Blog", new { slug = comment.Post.Slug }, _httpContext.Request.Url.Scheme);
                    await _notificationsService.SendNotificationAsync(comment.UserId, "Notification_CommentPublished", new { Url = postUrl });
                }
            }

            return comment;
        }

        protected virtual async Task<string> GetUserToNotifyReplyComment(int parentCommentId)
        {
            var comment = await FindByIdAsync(parentCommentId);
            if (comment.NotifyWhenReply && comment.UserId != null)
            {
                return comment.User.Id;
            }

            return "";
        }

        protected virtual async Task<List<string>> GetUsersToNotifyNewComment(int productId)
        {
            var result = await _dbContext.PostComments.Where(p => p.PostId == productId && p.NotifyWhenNewComment && p.UserId != null)
                .FromCacheAsync(QueryCacheTag.Comment);

            return result.Select(p => p.UserId).Distinct().ToList();
        }
    }
}