using System.Collections.Generic;
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
    public partial class PostTagsService : IPostTagsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostTagsService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<int> AddAsync(TblPostTags record)
        {
            record.Tag = record.Tag.Trim();
            if (!string.IsNullOrWhiteSpace(record.Tag))
            {
                _dbContext.PostTags.Add(record);
                await _dbContext.SaveChangesAsync();

                QueryCacheManager.ExpireTag(CacheTags.PostTag);

                _eventPublisher.EntityInserted(record);
            }
            return record.Id;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostTags.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostTags).Name, id);

            QueryCacheManager.ExpireTag(CacheTags.PostTag);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<List<TblPostTags>> TagSuggestionAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<TblPostTags>();
            }

            var result = await GetAsQueryable()
                .Where(p => p.Tag.Contains(query))
                .Take(10)
                .FromCacheAsync(CacheTags.PostTag);
            return result.ToList();
        }

        public virtual IQueryable<TblPostTags> GetAsQueryable()
        {
            return _dbContext.PostTags.OrderBy(p => p.Tag);
        }

        public virtual async Task<IEnumerable<TblPostTags>> GetAsEnumerableAsync()
        {
            return await GetAsQueryable().FromCacheAsync(CacheTags.PostTag);
        }

        public virtual async Task<TblPostTags> FindByIdAsync(int id)
        {
            var result = await _dbContext.PostTags
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.PostTag);
            return result;
        }

        public virtual async Task UpdateAsync(TblPostTags record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.PostTags.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.PostTag);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }
    }
}