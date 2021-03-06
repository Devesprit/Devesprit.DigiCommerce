using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.SEO;
using Devesprit.Services.Users;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostService<T> : IPostService<T> where T : TblPosts
    {
        protected readonly AppDbContext _dbContext;
        protected readonly ILocalizedEntityService _localizedEntityService;
        protected readonly IUserLikesService _userLikesService;
        protected readonly IUserWishlistService _userWishlistService;
        protected readonly IPostCategoriesService _categoriesService;
        protected readonly IEventPublisher _eventPublisher;
        protected readonly string _cacheKey;

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

            _cacheKey = typeof(T).Name;
        }

        public virtual IPagedList<T> GetItemsById(List<int> ids, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var skippedIds = ids.Skip(pageSize * (pageIndex - 1)).Take(pageSize).ToList();
            IQueryable<T> query = _dbContext.Set<T>().Where(p => p.Published && skippedIds.Contains(p.Id));

            var postsList = query
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Categories)
                .AsNoTracking()
                .FromCache(_cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage);
                
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
                query = query.Where(p => p.PublishDate >= fromDate);
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
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        CacheTags.PostCategory,
                        CacheTags.PostDescription,
                        CacheTags.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount()
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual List<SiteMapEntity> GetNewItemsForSiteMap()
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
                .AsNoTracking()
                .FromCache(_cacheKey);

            return result.ToList();
        }

        public virtual IPagedList<T> GetPopularItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published);
            if (fromDate != null)
            {
                query = query.Where(p => p.PublishDate >= fromDate);
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
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        CacheTags.PostCategory,
                        CacheTags.PostDescription,
                        CacheTags.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount()
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual IPagedList<T> GetHotList(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published && p.ShowInHotList);
            if (fromDate != null)
            {
                query = query.Where(p => p.PublishDate >= fromDate);
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
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        CacheTags.PostCategory,
                        CacheTags.PostDescription,
                        CacheTags.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount()
                    .FromCache(_cacheKey));

            return result;
        }

        public virtual IPagedList<T> GetFeaturedItems(int pageIndex = 1, int pageSize = int.MaxValue, int? filterByCategory = null, DateTime? fromDate = null)
        {
            var query = _dbContext.Set<T>().Where(p => p.Published && p.IsFeatured);
            if (fromDate != null)
            {
                query = query.Where(p => p.PublishDate >= fromDate);
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
                    .Include(p => p.Descriptions)
                    .Include(p => p.Images)
                    .Include(p => p.Categories)
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCache(_cacheKey,
                        CacheTags.PostCategory,
                        CacheTags.PostDescription,
                        CacheTags.PostImage),
                pageIndex,
                pageSize,
                query
                    .DeferredCount()
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
                .Include(p => p.Attributes.Select(x => x.PostAttribute))
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.Tags)
                .Include(p => p.AlternativeSlugs)
                .DeferredFirstOrDefault()
                .FromCacheAsync(
                    _cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage,
                    CacheTags.PostAttribute,
                    CacheTags.PostTag);
            return result;
        }

        public virtual async Task<T> FindBySlugAsync(string slug)
        {
            var result = await _dbContext.Set<T>()
                .Where(p => p.Slug == slug || p.AlternativeSlugs.Any(x => x.Slug == slug))
                .Include(p => p.Categories)
                .Include(p => p.Descriptions)
                .Include(p => p.Images)
                .Include(p => p.Attributes)
                .Include(p => p.Attributes.Select(x => x.PostAttribute))
                .Include(p => p.Attributes.Select(x => x.AttributeOption))
                .Include(p => p.Tags)
                .Include(p => p.AlternativeSlugs)
                .DeferredFirstOrDefault()
                .FromCacheAsync(
                    _cacheKey,
                    CacheTags.PostCategory,
                    CacheTags.PostDescription,
                    CacheTags.PostImage,
                    CacheTags.PostAttribute,
                    CacheTags.PostTag);
            return result;
        }

        public virtual IQueryable<T> GetAsQueryable()
        {
            return _dbContext.Set<T>().OrderByDescending(p => p.LastUpDate).ThenByDescending(p => p.PublishDate);
        }

        public virtual async Task DeleteAsync(int id)
        {
            await _userLikesService.DeletePostLikesAsync(id);
            await _userWishlistService.DeletePostFromWishlistAsync(id);
            await _dbContext.PostComments.Where(p => p.PostId == id).DeleteAsync();
            
            QueryCacheManager.ExpireTag(_cacheKey);
            var record = await FindByIdAsync(id);
            _dbContext.Set<T>().Remove(record);
            await _dbContext.SaveChangesAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(T).Name, id);

            //Remove from search engine
            DependencyResolver.Current.GetService<ISearchEngine>().DeletePostFromIndex(id);

            QueryCacheManager.ExpireTag(_cacheKey);
            MethodCache.ExpireTag(_cacheKey);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(T record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Set<T>().AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            //Set Post Tags & Categories & Alternative Slugs
            await UpdatePostSlugsAsync(record.Id, record.AlternativeSlugs?.Select(p => p.Slug).ToList());
            await UpdatePostTagsAsync(record.Id, record.Tags?.Select(p => p.Tag).ToList());
            await UpdatePostCategoriesAsync(record.Id, record.Categories?.Select(p => p.Id).ToList());

            //Update search engine
            var searchEngine = DependencyResolver.Current.GetService<ISearchEngine>();
            searchEngine.DeletePostFromIndex(record.Id);
            searchEngine.AddPostToIndex(record.Id);

            QueryCacheManager.ExpireTag(_cacheKey);
            MethodCache.ExpireTag(_cacheKey);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(T record)
        {
            var postSlugs = record.AlternativeSlugs?.Select(p => p.Slug).ToList();
            var postTags = record.Tags?.Select(p => p.Tag).ToList();
            var postCategories = record.Categories?.Select(p => p.Id).ToList();
            record.AlternativeSlugs = null;
            record.Tags = null;
            record.Categories = null;

            _dbContext.Set<T>().Add(record);
            await _dbContext.SaveChangesAsync();

            //Set Post Tags & Categories & Alternative Slugs
            await UpdatePostSlugsAsync(record.Id, postSlugs);
            await UpdatePostTagsAsync(record.Id, postTags);
            await UpdatePostCategoriesAsync(record.Id, postCategories);

            //Add to search engine
            DependencyResolver.Current.GetService<ISearchEngine>().AddPostToIndex(record.Id);

            QueryCacheManager.ExpireTag(_cacheKey);
            MethodCache.ExpireTag(_cacheKey);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task IncreaseNumberOfViewsAsync(T post, int value = 1)
        {
            post.NumberOfViews += value;
            _dbContext.Set<T>().AddOrUpdate(post);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task IncreaseNumberOfLikesAsync(T post, int value = 1)
        {
            post.NumberOfLikes += value;
            _dbContext.Set<T>().AddOrUpdate(post);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task UpdatePostSlugsAsync(int postId, List<string> slugsList)
        {
            await _dbContext.PostSlugs.Where(p => p.PostId == postId).DeleteAsync();
            slugsList = slugsList.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            foreach (var slugStr in slugsList)
            {
                _dbContext.PostSlugs.Add(new TblPostSlugs() { Slug = slugStr, PostId = postId});
            }
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
                QueryCacheManager.ExpireTag(CacheTags.PostTag);
                return;
            }

            tagsList = tagsList.Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
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
            QueryCacheManager.ExpireTag(CacheTags.PostTag);
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
                QueryCacheManager.ExpireTag(CacheTags.PostCategory);
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
            QueryCacheManager.ExpireTag(CacheTags.PostCategory);
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