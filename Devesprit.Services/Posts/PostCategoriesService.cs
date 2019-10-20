using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostCategoriesService : IPostCategoriesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostCategoriesService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblPostCategories> GetAsQueryable()
        {
            return _dbContext.PostCategories.OrderBy(p => p.DisplayOrder);
        }

        public virtual async Task<TblPostCategories> FindByIdAsync(int id)
        {
            var result = await _dbContext.PostCategories
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.PostCategory);
            return result;
        }

        public virtual async Task<TblPostCategories> FindBySlugAsync(string slug)
        {
            var result = await _dbContext.PostCategories
                .DeferredFirstOrDefault(p => p.Slug == slug)
                .FromCacheAsync(CacheTags.PostCategory);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostCategories.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostCategories).Name, id);
            QueryCacheManager.ExpireTag(CacheTags.PostCategory);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPostCategories record)
        {
            var allCategories = (await GetAsEnumerableAsync()).ToList();
            var oldRecord = allCategories.FirstOrDefault(p => p.Id == record.Id);
            allCategories.RemoveWhere(p => p.Id == record.Id);
            allCategories.Add(record);
            if (DetectLoop(allCategories, record, null))
                throw new Exception("Self referencing loop detected");

            _dbContext.PostCategories.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.PostCategory);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostCategories record)
        {
            _dbContext.PostCategories.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.PostCategory);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return GenerateCategoriesList((await GetAsEnumerableAsync()).ToList(), null);
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            return GenerateCategoriesList(GetAsEnumerable().ToList(), null);
        }

        public virtual async Task<IEnumerable<TblPostCategories>> GetAsEnumerableAsync()
        {
            return await GetAsQueryable().FromCacheAsync(CacheTags.PostCategory);
        }

        public virtual IEnumerable<TblPostCategories> GetAsEnumerable()
        {
            return GetAsQueryable().FromCache(CacheTags.PostCategory);
        }

        public virtual IEnumerable<TblPostCategories> GetCategoriesMustShowInFooter()
        {
            return GetAsQueryable().Where(p => p.ShowInFooter)
                .OrderBy(p => p.DisplayOrder)
                .FromCache(CacheTags.PostCategory);
        }

        public virtual async Task SetCategoryOrderAsync(int[] itemsOrder, int id, int? newParentId)
        {
            var nodeList = await _dbContext.PostCategories.ToListAsync();
            for (int i = 0; i < itemsOrder.Length; i++)
            {
                nodeList.First(p => p.Id == itemsOrder[i]).DisplayOrder = i;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.PostCategories.Where(p => p.Id == id).UpdateAsync(p => new TblPostCategories()
            {
                ParentCategoryId = newParentId
            });

            QueryCacheManager.ExpireTag(CacheTags.PostCategory);

            _eventPublisher.Publish(new PostCategoriesOrderChangeEvent(itemsOrder, id, newParentId));
        }

        public virtual List<int> GetSubCategories(int categoryId, List<int> result = null)
        {
            if (result == null)
            {
                result = new List<int> { categoryId };
            }

            var allCategories = GetAsEnumerable();

            foreach (var cat in allCategories.Where(p => p.ParentCategoryId == categoryId))
            {
                if (!result.Contains(cat.Id))
                {
                    result.Add(cat.Id);

                    GetSubCategories(cat.Id);
                }
            }

            return result;
        }

        protected virtual bool DetectLoop(List<TblPostCategories> categories, TblPostCategories category, HashSet<TblPostCategories> visited)
        {
            visited = visited ?? new HashSet<TblPostCategories>();
            var parentCategory = categories.FirstOrDefault(p => p.Id == category.ParentCategoryId);
            if (parentCategory != null)
            {
                if (!visited.Contains(parentCategory))
                {
                    visited.Add(parentCategory);
                    return DetectLoop(categories, parentCategory, visited);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual string GetCategoryName(List<TblPostCategories> allCategories, string categoryName, TblPostCategories currentCategory)
        {
            if (currentCategory.ParentCategoryId == null)
            {
                return categoryName;
            }
            
            return GetCategoryName(allCategories, "——" + categoryName,
                allCategories.FirstOrDefault(p => p.Id == currentCategory.ParentCategoryId.Value));
        }

        protected virtual List<SelectListItem> GenerateCategoriesList(List<TblPostCategories> allCategories, int? currentParentId)
        {
            var result = new List<SelectListItem>();
            foreach (var category in allCategories.Where(p => p.ParentCategoryId == currentParentId).OrderBy(p => p.DisplayOrder))
            {
                if (allCategories.Any(p => p.ParentCategoryId == category.Id))
                {
                    result.Add(new SelectListItem()
                    {
                        Value = category.Id.ToString(),
                        Text = GetCategoryName(allCategories, category.GetLocalized(p=> p.CategoryName), category)
                    });

                    result.AddRange(GenerateCategoriesList(allCategories, category.Id));
                }
                else
                {
                    result.Add(new SelectListItem()
                    {
                        Value = category.Id.ToString(),
                        Text = GetCategoryName(allCategories, category.GetLocalized(p => p.CategoryName), category)
                    });
                }
            }

            return result;
        }
    }
}