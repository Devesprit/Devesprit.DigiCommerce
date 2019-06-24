using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Countries
{
    public partial class CountriesService : ICountriesService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILanguagesService _languagesService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IWorkContext _workContext;
        private readonly IEventPublisher _eventPublisher;

        public CountriesService(AppDbContext dbContext, 
            IMemoryCache memoryCache, 
            ILanguagesService languagesService,
            ILocalizedEntityService localizedEntityService,
            IWorkContext workContext, 
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _languagesService = languagesService;
            _localizedEntityService = localizedEntityService;
            _workContext = workContext;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<List<Country>> GetAsEnumerableAsync()
        {
            // Try get result from cache (for ignore localizedProperty Cache expire)
            if (_memoryCache.Contains(QueryCacheTag.Country, _workContext.CurrentLanguage.IsoCode))
            {
                return _memoryCache.GetObject<List<Country>>(QueryCacheTag.Country, _workContext.CurrentLanguage.IsoCode);
            }

            var result = (await GetAsQueryable()
                .FromCacheAsync(QueryCacheTag.Country))
                .Select(p => new Country() {Id = p.Id, CountryName = p.GetLocalized(x => x.CountryName)})
                .ToList();
            
            _memoryCache.AddObject(QueryCacheTag.Country, result, TimeSpan.FromDays(30), _workContext.CurrentLanguage.IsoCode);
            return result;
        }

        public virtual IEnumerable<Country> GetAsEnumerable()
        {
            // Try get result from cache (for ignore localizedProperty Cache expire)
            if (_memoryCache.Contains(QueryCacheTag.Country, _workContext.CurrentLanguage.IsoCode))
            {
                return _memoryCache.GetObject<List<Country>>(QueryCacheTag.Country, _workContext.CurrentLanguage.IsoCode);
            }

            var result = GetAsQueryable()
                    .FromCache(QueryCacheTag.Country)
                .Select(p => new Country() { Id = p.Id, CountryName = p.GetLocalized(x => x.CountryName) })
                .ToList();

            _memoryCache.AddObject(QueryCacheTag.Country, result, TimeSpan.FromDays(30), _workContext.CurrentLanguage.IsoCode);
            return result;
        }

        public virtual async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() {Value = p.Id.ToString(), Text = p.CountryName})
                .ToList();
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            return GetAsEnumerable()
                .Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.CountryName })
                .ToList();
        }

        public virtual IQueryable<TblCountries> GetAsQueryable()
        {
            return _dbContext.Countries.OrderBy(p => p.CountryName);
        }

        public virtual async Task<TblCountries> FindByIdAsync(int id)
        {
            var result = await _dbContext.Countries
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Country);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Countries.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblCountries).Name, id);
            ClearCache();

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblCountries record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Countries.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            ClearCache();

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblCountries record)
        {
            _dbContext.Countries.Add(record);
            await _dbContext.SaveChangesAsync();
            ClearCache();

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        protected virtual void ClearCache()
        {
            QueryCacheManager.ExpireTag(QueryCacheTag.Country);
            _memoryCache.RemoveObject(QueryCacheTag.Country);
        }
    }
}