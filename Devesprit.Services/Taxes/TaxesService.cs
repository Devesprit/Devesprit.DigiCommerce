using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Taxes
{
    public partial class TaxesService : ITaxesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public TaxesService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<IEnumerable<TblTaxes>> GetAsEnumerableAsync()
        {
            var result = await GetAsQueryable()
                .FromCacheAsync(QueryCacheTag.Tax);
            return result;
        }

        public virtual IQueryable<TblTaxes> GetAsQueryable()
        {
            return _dbContext.Taxes;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Taxes.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblCountries).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.Tax);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<TblTaxes> FindByIdAsync(int id)
        {
            var result = await _dbContext.Taxes
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Tax);
            return result;
        }

        public virtual async Task<int> AddAsync(TblTaxes record)
        {
            _dbContext.Taxes.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Tax);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task UpdateAsync(TblTaxes record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Taxes.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.Tax);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }
    }
}