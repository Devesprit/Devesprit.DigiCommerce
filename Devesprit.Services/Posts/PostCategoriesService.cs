using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
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
            return _dbContext.PostCategories.OrderBy(p => p.CategoryName);
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
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.GetLocalized(x => x.CategoryName) })
                .OrderBy(p => p.Text)
                .ToList();
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            return GetAsEnumerable()
                .Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.GetLocalized(x => x.CategoryName) })
                .OrderBy(p => p.Text)
                .ToList();
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
                .OrderBy(p => p.CategoryName)
                .FromCache(CacheTags.PostCategory);
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
    }
}