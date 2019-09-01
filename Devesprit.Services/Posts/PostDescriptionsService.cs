using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostDescriptionsService : IPostDescriptionsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostDescriptionsService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblPostDescriptions> FindByIdAsync(int id)
        {
            return await _dbContext.PostDescriptions
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.PostDescription);
        }

        public virtual IQueryable<TblPostDescriptions> GetAsQueryable(int? filterByPostId)
        {
            IQueryable<TblPostDescriptions> result = _dbContext.PostDescriptions;
            if (filterByPostId != null)
            {
                result = result.Where(p => p.PostId == filterByPostId);
            }
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostDescriptions.Where(p => p.Id == id).DeleteAsync();
            _eventPublisher.EntityDeleted(record);

            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostDescriptions).Name, id);

            QueryCacheManager.ExpireTag(QueryCacheTag.PostDescription);
        }

        public virtual async Task UpdateAsync(TblPostDescriptions record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.PostDescriptions.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostDescription);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostDescriptions record)
        {
            _dbContext.PostDescriptions.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostDescription);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }
    }
}