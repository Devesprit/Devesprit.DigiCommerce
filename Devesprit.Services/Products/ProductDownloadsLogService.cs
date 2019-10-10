using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Products
{
    public partial class ProductDownloadsLogService : IProductDownloadsLogService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public ProductDownloadsLogService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblProductDownloadsLog> GetAsQueryable()
        {
            return _dbContext.ProductDownloadsLog.OrderByDescending(p=> p.DownloadDate);
        }

        public virtual async Task<TblProductDownloadsLog> FindByIdAsync(int id)
        {
            return await _dbContext.ProductDownloadsLog
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.ProductDownloadLog);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.ProductDownloadsLog.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.ProductDownloadLog);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task AddAsync(TblProductDownloadsLog log)
        {
            _dbContext.ProductDownloadsLog.Add(log);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.ProductDownloadLog);

            _eventPublisher.EntityInserted(log);
        }

        public virtual int GetNumberOfDownloads()
        {
            return _dbContext.ProductDownloadsLog.DeferredCount().FromCache(CacheTags.ProductDownloadLog);
        }
    }
}