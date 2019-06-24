using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostAttributesMappingService : IPostAttributesMappingService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostAttributesMappingService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblPostAttributesMapping> FindByIdAsync(int id)
        {
            return await _dbContext.PostAttributesMapping
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.PostAttribute);
        }

        public virtual IQueryable<TblPostAttributesMapping> GetAsQueryable(int productId)
        {
            return _dbContext.PostAttributesMapping.Where(p => p.PostId == productId);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostAttributesMapping.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostAttributesMapping).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPostAttributesMapping record)
        {
            var oldRecord = await FindByIdAsync(record.Id);

            _dbContext.PostAttributesMapping.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostAttributesMapping record)
        {
            _dbContext.PostAttributesMapping.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }
    }
}