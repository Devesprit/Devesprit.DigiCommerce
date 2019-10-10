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
    public partial class PostImagesService : IPostImagesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostImagesService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblPostImages> FindByIdAsync(int id)
        {
            return await _dbContext.PostImages
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.PostImage);
        }

        public virtual IQueryable<TblPostImages> GetAsQueryable(int? filterByPostId)
        {
            IQueryable<TblPostImages> result = _dbContext.PostImages;
            if (filterByPostId != null )
            {
                result = result.Where(p => p.PostId == filterByPostId);
            }
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostImages.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostImages).Name, id);

            QueryCacheManager.ExpireTag(CacheTags.PostImage);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPostImages record)
        {
            var oldRecord = await FindByIdAsync(record.Id);

            _dbContext.PostImages.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.PostImage);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostImages record)
        {
            _dbContext.PostImages.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.PostImage);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }
    }
}