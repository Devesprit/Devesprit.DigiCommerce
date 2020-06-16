using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Settings;
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
        private readonly ISettingService _settingService;

        public UserLikesService(AppDbContext dbContext,
            IEventPublisher eventPublisher,
            ISettingService settingService)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _settingService = settingService;
        }

        public virtual IQueryable<TblUserLikes> GetAsQueryable()
        {
            return _dbContext.UserLikes.OrderByDescending(p => p.Date);
        }

        public virtual async Task<TblUserLikes> FindByIdAsync(int id)
        {
            return await _dbContext.UserLikes
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.UserLikes);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserLikes.Where(p=> p.Id == id).DeleteAsync();
            
            QueryCacheManager.ExpireTag(CacheTags.UserLikes);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task DeletePostLikesAsync(int postId)
        {
            var records = (await _dbContext.UserLikes.Where(p => p.PostId == postId)
                .FromCacheAsync(CacheTags.UserLikes)).ToList();
            if (records.Any())
            {
                var recordIds = records.Select(x => x.Id).ToList();
                await _dbContext.UserLikes.Where(p => recordIds.Contains(p.Id)).DeleteAsync();

                QueryCacheManager.ExpireTag(CacheTags.UserLikes);

                records.ForEach(p => _eventPublisher.EntityDeleted(p));
            }
        }

        public virtual async Task AddAsync(TblUserLikes like)
        {
            _dbContext.UserLikes.Add(like);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserLikes);

            _eventPublisher.EntityInserted(like);
        }

        public virtual async Task<bool> LikePostAsync(int postId, string userId, PostType? postType)
        {
            if (!(await _settingService.LoadSettingAsync<SiteSettings>()).LikePosts)
            {
                return false;
            }

            var alreadyLiked = (await _dbContext.UserLikes.Where(p => p.PostId == postId && p.UserId == userId)
                .FromCacheAsync(CacheTags.UserLikes)).ToList();
            if (alreadyLiked.Any())
            {
                await _dbContext.UserLikes.Where(p => p.PostId == postId && p.UserId == userId).DeleteAsync();
                QueryCacheManager.ExpireTag(CacheTags.UserLikes);

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

            if (!_settingService.LoadSetting<SiteSettings>().LikePosts)
            {
                return false;
            }

            return _dbContext.UserLikes.Where(p => p.UserId == userId).FromCache(CacheTags.UserLikes)
                .Any(p => p.PostId == postId);
        }

        public virtual Dictionary<int, bool> UserLikedThisPost(int[] postIds, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return postIds.ToDictionary(p => p, p => false);
            }

            if (!_settingService.LoadSetting<SiteSettings>().LikePosts)
            {
                return postIds.ToDictionary(p => p, p => false);
            }

            var records = _dbContext.UserLikes
                .Where(p => postIds.Contains(p.PostId) && p.UserId == userId)
                .FromCache(CacheTags.UserLikes)
                .ToList();

            return postIds.ToDictionary(postId => postId, postId => records.Any(p => p.PostId == postId));
        }

        public virtual int GetNumberOfLikes(int postId)
        {
            return GetAsQueryable()
                .DeferredCount(p => p.PostId == postId)
                .FromCache(DateTimeOffset.Now.AddHours(24));
        }

        public virtual Dictionary<int, int> GetNumberOfLikes(int[] postIds)
        {
            var res = GetAsQueryable()
                .Where(p => postIds.Contains(p.PostId))
                .GroupBy(p => p.PostId)
                .Select(n => new
                    {
                        PostId = n.Key,
                        LikeCount = n.Count()
                    }
                ).FromCache(DateTimeOffset.Now.AddHours(24));

            return res.ToDictionary(p => p.PostId, p => p.LikeCount);
        }
    }
}