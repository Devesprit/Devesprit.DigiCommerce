using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Products
{
    public partial class ProductDiscountsForUserGroupsService : IProductDiscountsForUserGroupsService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public ProductDiscountsForUserGroupsService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblProductDiscountsForUserGroups> FindByIdAsync(int id)
        {
            return await _dbContext.ProductDiscountsForUserGroups
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.ProductDiscountForUserGroup);
        }

        public virtual IQueryable<TblProductDiscountsForUserGroups> GetAsQueryable(int? filterByProductId)
        {
            IQueryable<TblProductDiscountsForUserGroups> result = _dbContext.ProductDiscountsForUserGroups;
            if (filterByProductId != null)
            {
                result = result.Where(p => p.ProductId == filterByProductId);
            }
            return result;
        }

        public virtual IEnumerable<TblProductDiscountsForUserGroups> FindProductDiscounts(int productId)
        {
            return _dbContext.ProductDiscountsForUserGroups
                .Where(p => p.ProductId == productId)
                .OrderByDescending(p => p.DiscountPercent)
                .Include(p => p.UserGroup)
                .FromCache(new []{CacheTags.ProductDiscountForUserGroup,
                    CacheTags.UserGroup});
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.ProductDiscountsForUserGroups.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.ProductDiscountForUserGroup);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblProductDiscountsForUserGroups record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.ProductDiscountsForUserGroups.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.ProductDiscountForUserGroup);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblProductDiscountsForUserGroups record)
        {
            _dbContext.ProductDiscountsForUserGroups.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.ProductDiscountForUserGroup);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }
    }
}