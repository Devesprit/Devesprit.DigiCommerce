using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Pages
{
    public partial class PagesService : IPagesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PagesService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblPages> GetAsQueryable()
        {
            return _dbContext.Pages;
        }

        public virtual async Task<TblPages> FindByIdAsync(int id)
        {
            var result = await _dbContext.Pages
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Page);
            return result;
        }

        public virtual async Task<TblPages> FindBySlugAsync(string slug)
        {
            var result = await _dbContext.Pages
                .DeferredFirstOrDefault(p => p.Slug == slug)
                .FromCacheAsync(QueryCacheTag.Page);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Pages.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPages).Name, id);   
            QueryCacheManager.ExpireTag(QueryCacheTag.Page);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPages record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            if (record.ShowAsWebsiteDefaultPage && record.Published)
            {
                await _dbContext.Pages.Where(p => p.ShowAsWebsiteDefaultPage)
                    .UpdateAsync(p => new TblPages() {ShowAsWebsiteDefaultPage = false});
            }

            _dbContext.Pages.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Page);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPages record)
        {
            if (record.ShowAsWebsiteDefaultPage && record.Published)
            {
                await _dbContext.Pages.Where(p => p.ShowAsWebsiteDefaultPage)
                    .UpdateAsync(p => new TblPages() { ShowAsWebsiteDefaultPage = false });
            }

            _dbContext.Pages.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Page);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task<IEnumerable<TblPages>> GetAsEnumerableAsync()
        {
            var result = await GetAsQueryable()
                .FromCacheAsync(QueryCacheTag.Page);
            return result;
        }

        public virtual IEnumerable<TblPages> GetPagesMustShowInFooter()
        {
            var result = GetAsQueryable().Where(p => p.ShowInFooter && p.Published)
                .OrderBy(p=> p.Title)
                .FromCache(QueryCacheTag.Page);
            return result;
        }

        public virtual IEnumerable<TblPages> GetPagesMustShowInUserMenuBar()
        {
            var result = GetAsQueryable().Where(p => p.ShowInUserMenuBar && p.Published)
                .OrderBy(p => p.Title)
                .FromCache(QueryCacheTag.Page);
            return result;
        }

        public virtual async Task<TblPages> GetWebsiteDefaultPageAsync()
        {
            var result = await _dbContext.Pages
                .DeferredFirstOrDefault(p => p.ShowAsWebsiteDefaultPage && p.Published)
                .FromCacheAsync(QueryCacheTag.Page);
            return result;
        }
    }
}