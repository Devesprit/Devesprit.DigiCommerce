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
    public partial class UserWishlistService : IUserWishlistService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public UserWishlistService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblUserWishlist> GetAsQueryable()
        {
            return _dbContext.UserWishlist.OrderByDescending(p => p.Date);
        }

        public virtual async Task<TblUserWishlist> FindByIdAsync(int id)
        {
            return await _dbContext.UserWishlist
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.UserWishlist);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserWishlist.Where(p => p.Id == id).DeleteAsync();

            QueryCacheManager.ExpireTag(QueryCacheTag.UserWishlist);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task DeletePostFromWishlistAsync(int postId)
        {
            var records = (await _dbContext.UserWishlist.Where(p => p.PostId == postId)
                .FromCacheAsync(QueryCacheTag.UserWishlist)).ToList();
            if (records.Any())
            {
                var recordIds = records.Select(x => x.Id).ToList();
                await _dbContext.UserWishlist.Where(p => recordIds.Contains(p.Id)).DeleteAsync();

                QueryCacheManager.ExpireTag(QueryCacheTag.UserWishlist);

                records.ForEach(p => _eventPublisher.EntityDeleted(p));
            }
        }

        public virtual async Task AddAsync(TblUserWishlist record)
        {
            _dbContext.UserWishlist.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(QueryCacheTag.UserWishlist);

            _eventPublisher.EntityInserted(record);
        }

        public virtual async Task<bool> AddPostToUserWishlistAsync(int postId, string userId, PostType? postType)
        {
            var alreadyLiked = (await _dbContext.UserWishlist.Where(p => p.PostId == postId && p.UserId == userId)
                .FromCacheAsync(QueryCacheTag.UserWishlist)).ToList();
            if (alreadyLiked.Any())
            {
                await _dbContext.UserWishlist.Where(p => p.PostId == postId && p.UserId == userId).DeleteAsync();
                QueryCacheManager.ExpireTag(QueryCacheTag.UserWishlist);

                alreadyLiked.ForEach(x => _eventPublisher.EntityDeleted(x));

                return false;
            }

            await AddAsync(new TblUserWishlist() { Date = DateTime.Now, PostId = postId, UserId = userId, PostType = postType });
            return true;
        }

        public virtual bool UserAddedThisPostToWishlist(int postId, string userId)
        {
            if (postId <= 0 || string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            return _dbContext.UserWishlist.Where(p => p.UserId == userId).FromCache(QueryCacheTag.UserWishlist)
                .Any(p => p.PostId == postId);
        }
    }
}