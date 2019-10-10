using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Countries
{
    public partial class CountriesService : ICountriesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public CountriesService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<List<Country>> GetAsEnumerableAsync()
        {
            var result = (await GetAsQueryable()
                .FromCacheAsync(CacheTags.Country))
                .Select(p => new Country() {Id = p.Id, CountryName = p.GetLocalized(x => x.CountryName)})
                .ToList();
            
            return result;
        }

        public virtual IEnumerable<Country> GetAsEnumerable()
        {
            var result = GetAsQueryable()
                    .FromCache(CacheTags.Country)
                .Select(p => new Country() { Id = p.Id, CountryName = p.GetLocalized(x => x.CountryName) })
                .ToList();

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
                .FromCacheAsync(CacheTags.Country);
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
            QueryCacheManager.ExpireTag(CacheTags.Country);
            MethodCache.ExpireTag(CacheTags.Country);
        }
    }
}