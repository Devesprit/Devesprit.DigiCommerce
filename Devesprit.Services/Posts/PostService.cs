using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.SEO;
using Devesprit.Services.Users;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostService<T> : IPostService<T> where T : TblPosts
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUserLikesService _userLikesService;
        private readonly IUserWishlistService _userWishlistService;
        private readonly IPostCategoriesService _categoriesService;
        private readonly IEventPublisher _eventPublisher;
        private readonly string _cacheKey;

        public PostService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IPostCategoriesService categoriesService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _userLikesService = userLikesService;
            _userWishlistService = userWishlistService;
            _categoriesService = categoriesService;
            _eventPublisher = eventPublisher;

            _cacheKey = nameof(T);
        }

        public virtual IPagedList<T> GetItemsById(List<int> ids, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var skippedIds = ids.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            IQueryable<T> query = _dbContext.Set<T>().Where(p => p.Published && skippedIds.Contains(p.Id));

            var postsList = query
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Categories)
                .FromCache(_cacheKey,
                    QueryCacheTag.PostCategory,
                    QueryCacheTag.PostDescription,
                    QueryCacheTag.PostImage);
                
            postsList = postsList.OrderBy(p => skippedIds.IndexOf(p.Id));

            var result = new StaticPagedList<T>(
                postsList,
                pageIndex,
                pageSize,
                ids.Count);

            return result;
        }

        public virtual IPagedList<T> GetNewItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);
            if (fromDate != null)
            {
                query = _dbContext.Set<T>().Where(p => p.PublishDate >= fromDate);
            }
            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var result = new StaticPagedList<T>(
                query
                    .OrderByDescending(p => p.PinToTop)
                    .ThenByDescending(p => p.LastUpDate)
                    .ThenByDescending(p => p.PublishDate)
                    .Include(p => p.Categories)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        QueryCacheTag.PostCategory,
                        QueryCacheTag.PostDescription,
                        QueryCacheTag.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount()
                    .FromCache(_cacheKey));

            return result;
        }

        public List<SiteMapEntity> GetNewItemsForSiteMap()
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);

            var result = query
                .OrderByDescending(p => p.PinToTop)
                .ThenByDescending(p => p.LastUpDate)
                .ThenByDescending(p => p.PublishDate)
                .Select(p => new SiteMapEntity()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    LastUpDate = p.LastUpDate,
                    PublishDate = p.PublishDate,
                    PostType = p.PostType
                })
                .FromCache(_cacheKey,
                    QueryCacheTag.PostCategory,
                    QueryCacheTag.PostDescription,
                    QueryCacheTag.PostImage);

            return result.ToList();
        }

        public virtual IPagedList<T> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);
            if (fromDate != null)
            {
                query = _dbContext.Set<T>().Where(p => p.PublishDate >= fromDate);
            }

            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var result = new StaticPagedList<T>(
                query
                    .OrderByDescending(p => p.NumberOfViews)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        QueryCacheTag.PostCategory,
                        QueryCacheTag.PostDescription,
                        QueryCacheTag.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount(p => p.Published)
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual IPagedList<T> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);
            if (fromDate != null)
            {
                query = _dbContext.Set<T>().Where(p => p.PublishDate >= fromDate);
            }

            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var result = new StaticPagedList<T>(
                query
                    .Where(p => p.ShowInHotList)
                    .OrderByDescending(p => p.PinToTop)
                    .ThenByDescending(p => p.LastUpDate)
                    .ThenByDescending(p => p.PublishDate)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        QueryCacheTag.PostCategory,
                        QueryCacheTag.PostDescription,
                        QueryCacheTag.PostImage),
                pageIndex,
                pageSize,
                _dbContext.Set<T>()
                    .DeferredCount(p => p.ShowInHotList && p.Published)
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual IPagedList<T> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);
            if (fromDate != null)
            {
                query = _dbContext.Set<T>().Where(p => p.PublishDate >= fromDate);
            }

            if (filterByCategory != null)
            {
                var subCategories = _categoriesService.GetSubCategories(filterByCategory.Value);
                query = query.Where(p => p.Categories.Any(x => subCategories.Contains(x.Id)));
            }

            var result = new StaticPagedList<T>(
                query
                    .Where(p => p.IsFeatured)
                    .OrderByDescending(p => p.PinToTop)
                    .ThenByDescending(p => p.LastUpDate)
                    .ThenByDescending(p => p.PublishDate)
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        QueryCacheTag.PostCategory,
                        QueryCacheTag.PostDescription,
                        QueryCacheTag.PostImage),
                pageIndex,
                pageSize,
                _dbContext.Set<T>()
                    .DeferredCount(p => p.ShowInHotList && p.Published)
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual async Task<T> FindByIdAsync(int id)
        {
            var result = await _dbContext.Set<T>()
                .Where(p => p.Id == id)
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.Tags)
                .DeferredFirstOrDefault()
                .FromCacheAsync(
                    _cacheKey,
                    QueryCacheTag.PostCategory,
                    QueryCacheTag.PostDescription,
                    QueryCacheTag.PostImage,
                    QueryCacheTag.PostAttribute,
                    QueryCacheTag.PostTag);
            return result;
        }

        public virtual async Task<T> FindBySlugAsync(string slug)
        {
            var result = await _dbContext.Set<T>()
                .Where(p => p.Slug == slug)
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.Tags)
                .DeferredFirstOrDefault()
                .FromCacheAsync(
                    _cacheKey,
                    QueryCacheTag.PostCategory,
                    QueryCacheTag.PostDescription,
                    QueryCacheTag.PostImage,
                    QueryCacheTag.PostAttribute,
                    QueryCacheTag.PostTag);
            return result;
        }

        public virtual IQueryable<T> GetAsQueryable()
        {
            return _dbContext.Set<T>().OrderByDescending(p => p.LastUpDate).ThenByDescending(p => p.PublishDate);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _userLikesService.DeletePostLikesAsync(id);
            await _userWishlistService.DeletePostFromWishlistAsync(id);
            await _dbContext.PostComments.Where(p => p.PostId == id).DeleteAsync();
            var post = await FindByIdAsync(id);
            _dbContext.Set<T>().Remove(post);
            await _dbContext.SaveChangesAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(T).Name, id);

            QueryCacheManager.ExpireTag(_cacheKey);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(T record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Set<T>().AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            //Set Post Tags & Categories
            await UpdatePostTagsAsync(record.Id, record.Tags?.Select(p => p.Tag).ToList());
            await UpdatePostCategoriesAsync(record.Id, record.Categories?.Select(p => p.Id).ToList());

            QueryCacheManager.ExpireTag(_cacheKey);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(T record)
        {
            var postTags = record.Tags?.Select(p => p.Tag).ToList();
            var postCategories = record.Categories?.Select(p => p.Id).ToList();
            record.Tags = null;
            record.Categories = null;

            _dbContext.Set<T>().Add(record);
            await _dbContext.SaveChangesAsync();

            //Set Post Tags & Categories
            await UpdatePostTagsAsync(record.Id, postTags);
            await UpdatePostCategoriesAsync(record.Id, postCategories);

            QueryCacheManager.ExpireTag(_cacheKey);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task IncreaseNumberOfViewsAsync(T post, int value = 1)
        {
            post.NumberOfViews += value;
            await _dbContext.SaveChangesAsync();
        }
        
        public virtual async Task UpdatePostTagsAsync(int postId, List<string> tagsList)
        {
            var post = await _dbContext.Set<T>()
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null) return;

            _eventPublisher.Publish(new PostTagsUpdatedEvent(post, tagsList));

            if (tagsList == null || tagsList.Count == 0)
            {
                post.Tags.Clear();
                await _dbContext.SaveChangesAsync();
                QueryCacheManager.ExpireTag(QueryCacheTag.PostTag);
                return;
            }

            tagsList = tagsList.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
            var existingTags = await _dbContext.PostTags
                .Where(p => tagsList.Contains(p.Tag)).ToListAsync();

            post.Tags.Clear();

            foreach (var tagStr in tagsList)
            {
                var tag = existingTags.FirstOrDefault(p =>
                              string.Compare(p.Tag, tagStr, StringComparison.InvariantCultureIgnoreCase) == 0) ??
                          _dbContext.PostTags.Add(new TblPostTags() { Tag = tagStr });

                post.Tags.Add(tag);
            }

            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostTag);
        }

        public virtual async Task UpdatePostCategoriesAsync(int postId, List<int> categoriesList)
        {
            var post = await _dbContext.Set<T>()
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null) return;

            _eventPublisher.Publish(new PostCategoriesUpdatedEvent(post, categoriesList));

            if (categoriesList == null || categoriesList.Count == 0)
            {
                post.Categories.Clear();
                await _dbContext.SaveChangesAsync();
                QueryCacheManager.ExpireTag(QueryCacheTag.PostCategory);
                return;
            }

            var existingCategories = await _dbContext.PostCategories
                .Where(p => categoriesList.Contains(p.Id)).ToListAsync();

            post.Categories.Clear();

            foreach (var catId in categoriesList.Distinct())
            {
                var category = existingCategories.FirstOrDefault(p => p.Id == catId);

                post.Categories.Add(category);
            }

            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostCategory);
        }

        public virtual void GetStatics(out int numberOfPosts, out int numberOfVisits, out DateTime lastUpdate)
        {
            numberOfPosts = _dbContext.Set<T>().DeferredCount(p => p.Published).FromCache(_cacheKey);
            try
            {
                numberOfVisits = _dbContext.Set<T>().Where(p => p.Published).DeferredSum(p => p.NumberOfViews).FromCache(_cacheKey);
            }
            catch
            {
                numberOfVisits = 0;
            }
            var lastUpdatedPost = _dbContext.Set<T>().OrderByDescending(p => p.LastUpDate).DeferredFirstOrDefault()
                    .FromCache(_cacheKey)?.LastUpDate;
            var latestPost = _dbContext.Set<T>().OrderByDescending(p => p.PublishDate).DeferredFirstOrDefault()
                .FromCache(_cacheKey)?.PublishDate;
            if (lastUpdatedPost > latestPost)
            {
                lastUpdate = lastUpdatedPost.Value;
            }
            else
            {
                lastUpdate = latestPost ?? DateTime.MinValue;
            }
        }
    }
}