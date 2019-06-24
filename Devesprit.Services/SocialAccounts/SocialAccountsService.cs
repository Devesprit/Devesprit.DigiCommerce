using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.SocialAccounts
{
    public partial class SocialAccountsService : ISocialAccountsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public SocialAccountsService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblSocialAccounts> GetAsQueryable()
        {
            return _dbContext.SocialAccounts;
        }

        public virtual IEnumerable<TblSocialAccounts> GetAsEnumerable()
        {
            return _dbContext.SocialAccounts.FromCache(QueryCacheTag.SocialAccounts);
        }

        public virtual async Task<TblSocialAccounts> FindByIdAsync(int id)
        {
            var result = await _dbContext.SocialAccounts
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.SocialAccounts);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.SocialAccounts.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblSocialAccounts).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.SocialAccounts);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblSocialAccounts record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.SocialAccounts.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.SocialAccounts);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblSocialAccounts record)
        {
            _dbContext.SocialAccounts.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.SocialAccounts);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }
    }
}