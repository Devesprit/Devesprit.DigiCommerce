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
using Devesprit.Services.Localization;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Currency
{
    public partial class CurrencyService: ICurrencyService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public CurrencyService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }
        
        public virtual IEnumerable<TblCurrencies> GetAsEnumerable()
        {
            return _dbContext.Currencies.Where(p => p.Published).OrderBy(p => p.DisplayOrder)
                .FromCache(QueryCacheTag.Currency);
        }

        public virtual IQueryable<TblCurrencies> GetAsQueryable()
        {
            return _dbContext.Currencies.OrderBy(p => p.DisplayOrder);
        }

        public virtual async Task<IEnumerable<TblCurrencies>> GetAsEnumerableAsync()
        {
            var result = await _dbContext.Currencies.Where(p => p.Published).OrderBy(p => p.DisplayOrder)
                .FromCacheAsync(QueryCacheTag.Currency);
            return result;
        }

        public virtual async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.GetLocalized(x=> x.CurrencyName) })
                .ToList();
        }

        public virtual TblCurrencies GetDefaultCurrency()
        {
            return _dbContext.Currencies
                       .DeferredFirstOrDefault(p => p.IsMainCurrency && p.Published)
                       .FromCache(QueryCacheTag.Currency) ??
                   _dbContext.Currencies
                       .DeferredFirstOrDefault(p => p.Published).FromCache(QueryCacheTag.Currency);
        }

        public virtual async Task<TblCurrencies> FindByIdAsync(int id)
        {
            var result = await _dbContext.Currencies
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Currency);
            return result;
        }

        public virtual TblCurrencies FindByIso(string iso)
        {
            return _dbContext.Currencies
                .DeferredFirstOrDefault(p => p.IsoCode.Trim().ToLower() == iso.Trim().ToLower())
                .FromCache(QueryCacheTag.Currency);
        }

        public virtual List<string> GetAllCurrenciesIsoList()
        {
            return _dbContext.Currencies.Where(p => p.Published).Select(p => p.IsoCode.ToLower().Trim())
                .FromCache(QueryCacheTag.Currency).ToList();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            if (record != null && record.IsMainCurrency)
            {
                throw new Exception($"You can not delete system default currency (Id: {id}).");
            }

            await _dbContext.Currencies.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(record);
            QueryCacheManager.ExpireTag(QueryCacheTag.Currency);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblCurrencies record)
        {
            if (await _dbContext.Languages
                .DeferredAny(p => p.IsoCode.Trim() == record.IsoCode.Trim() && p.Id != record.Id)
                .FromCacheAsync(QueryCacheTag.Currency))
            {
                throw new Exception($"The \"{record.IsoCode}\" ISO code already exist.");
            }

            var oldRecord = await FindByIdAsync(record.Id);
            
            if (oldRecord != null)
            {
                if (oldRecord.IsMainCurrency && !record.Published)
                {
                    throw new Exception("System default currency cannot be at unpublished status.");
                }

                _dbContext.Currencies.AddOrUpdate(record);
                await _dbContext.SaveChangesAsync();

                QueryCacheManager.ExpireTag(QueryCacheTag.Currency);

                _eventPublisher.EntityUpdated(record, oldRecord);
            }
        }

        public virtual async Task<int> AddAsync(TblCurrencies record)
        {
            if (await _dbContext.Languages.DeferredAny(p => p.IsoCode.Trim() == record.IsoCode.Trim())
                .FromCacheAsync(QueryCacheTag.Currency))
            {
                throw new Exception($"The \"{record.IsoCode}\" ISO code already exist.");
            }

            _dbContext.Currencies.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Currency);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task SetAsDefaultAsync(int id)
        {
            var record = await _dbContext.Currencies.FirstOrDefaultAsync(p => p.Id == id);
            if (record == null)
            {
                throw new Exception("Invalid currency Id.");
            }
            if (!record.Published)
            {
                throw new Exception("Unpublished currency cannot be used as the default currency.");
            }

            await _dbContext.Currencies
                .UpdateAsync(p => new TblCurrencies() { IsMainCurrency = false });
            await _dbContext.Currencies.Where(p => p.Id == id)
                .UpdateAsync(p => new TblCurrencies() { IsMainCurrency = true });

            QueryCacheManager.ExpireTag(QueryCacheTag.Currency);

            _eventPublisher.Publish(new DefaultCurrencyChangeEvent(record));
        }
    }
}
