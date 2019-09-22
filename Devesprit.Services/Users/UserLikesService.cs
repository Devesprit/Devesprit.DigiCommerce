using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Posts;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Users
{
    public partial class UserLikesService : IUserLikesService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public UserLikesService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblUserLikes> GetAsQueryable()
        {
            return _dbContext.UserLikes.OrderByDescending(p => p.Date);
        }

        public virtual async Task<TblUserLikes> FindByIdAsync(int id)
        {
            return await _dbContext.UserLikes
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.UserLikes);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserLikes.Where(p=> p.Id == id).DeleteAsync();
            
            QueryCacheManager.ExpireTag(QueryCacheTag.UserLikes);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task DeletePostLikesAsync(int postId)
        {
            var records = (await _dbContext.UserLikes.Where(p => p.PostId == postId)
                .FromCacheAsync(QueryCacheTag.UserLikes)).ToList();
            if (records.Any())
            {
                var recordIds = records.Select(x => x.Id).ToList();
                await _dbContext.UserLikes.Where(p => recordIds.Contains(p.Id)).DeleteAsync();

                QueryCacheManager.ExpireTag(QueryCacheTag.UserLikes);

                records.ForEach(p => _eventPublisher.EntityDeleted(p));
            }
        }

        public virtual async Task AddAsync(TblUserLikes like)
        {
            _dbContext.UserLikes.Add(like);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(QueryCacheTag.UserLikes);

            _eventPublisher.EntityInserted(like);
        }

        public virtual async Task<bool> LikePostAsync(int postId, string userId, PostType? postType)
        {
            var alreadyLiked = (await _dbContext.UserLikes.Where(p => p.PostId == postId && p.UserId == userId)
                .FromCacheAsync(QueryCacheTag.UserLikes)).ToList();
            if (alreadyLiked.Any())
            {
                await _dbContext.UserLikes.Where(p => p.PostId == postId && p.UserId == userId).DeleteAsync();
                QueryCacheManager.ExpireTag(QueryCacheTag.UserLikes);

                alreadyLiked.ForEach(x => _eventPublisher.EntityDeleted(x));

                return false;
            }

            await AddAsync(new TblUserLikes(){Date = DateTime.Now, PostId = postId, UserId = userId, PostType = postType });
            return true;
        }
        
        public virtual bool UserLikedThisPost(int postId, string userId)
        {
            if (postId <= 0 || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return _dbContext.UserLikes.Where(p => p.UserId == userId).FromCache(QueryCacheTag.UserLikes)
                .Any(p => p.PostId == postId);
        }

        public virtual int GetPostNumberOfLikes(int postId)
        {
            return GetAsQueryable()
                .DeferredCount(p => p.PostId == postId)
                .FromCache(DateTimeOffset.Now.AddHours(24));
        }
    }
}