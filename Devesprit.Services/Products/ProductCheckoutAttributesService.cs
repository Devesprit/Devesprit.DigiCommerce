using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Products
{
    public partial class ProductCheckoutAttributesService : IProductCheckoutAttributesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public ProductCheckoutAttributesService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblProductCheckoutAttributes> FindByIdAsync(int id)
        {
            return await _dbContext.ProductCheckoutAttributes
                .Include(p => p.Options)
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.ProductCheckoutAttribute);
        }

        public virtual IQueryable<TblProductCheckoutAttributes> GetAsQueryable(int? filterByProductId)
        {
            IQueryable<TblProductCheckoutAttributes> result = _dbContext.ProductCheckoutAttributes;
            if (filterByProductId != null)
            {
                result = result.Where(p => p.ProductId == filterByProductId);
            }
            return result;
        }

        public virtual async Task<IEnumerable<TblProductCheckoutAttributes>> FindProductAttributesAsync(int productId)
        {
            return await _dbContext.ProductCheckoutAttributes
                .Where(p => p.ProductId == productId)
                .Include(p => p.Options)
                .FromCacheAsync(QueryCacheTag.ProductCheckoutAttribute);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.ProductCheckoutAttributes.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(
                typeof(TblProductCheckoutAttributes).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblProductCheckoutAttributes record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.ProductCheckoutAttributes.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblProductCheckoutAttributes record)
        {
            _dbContext.ProductCheckoutAttributes.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task<double> CalculateAttributeOptionPriceForUserAsync(int optionId, TblUsers user)
        {
            var option = await FindOptionByIdAsync(optionId);
            if (option == null)
            {
                throw new ArgumentNullException(nameof(optionId));
            }

            var result = option.Price;

            if (result <= 0)
            {
                return 0;
            }

            var alreadyPurchased = user?.Invoices.Where(p => p.Status == InvoiceStatus.Paid)
                .SelectMany(p => p.InvoiceDetails).Where(p => p.ItemType == InvoiceDetailsItemType.ProductAttributeOption &&
                                                              p.ItemId == optionId).ToList();
            if (alreadyPurchased != null &&
                alreadyPurchased.OrderByDescending(p => p.PurchaseExpiration).Any(p =>
                    DateTime.Now <= p.PurchaseExpiration))
            {
                result = option.RenewalPrice;
            }

            var userGroupsDiscount = await
                option.ProductCheckoutAttribute.Product.DiscountsForUserGroups.Where(p =>
                    p.ApplyDiscountToProductAttributes).ToListAsync();

            if (user?.UserGroup != null && userGroupsDiscount.Any())
            {
                var discount =
                    userGroupsDiscount.FirstOrDefault(p => p.UserGroupId == user.UserGroupId);
                if (discount != null)
                {
                    result = result - ((result * discount.DiscountPercent) / 100);
                    if (result < 0)
                    {
                        result = 0;
                    }
                }
            }

            return result;
        }


        #region Attribute Options

        public virtual async Task<TblProductCheckoutAttributeOptions> FindOptionByIdAsync(int id)
        {
            return await _dbContext.ProductCheckoutAttributeOptions
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.ProductCheckoutAttribute);
        }

        public virtual IQueryable<TblProductCheckoutAttributeOptions> GetOptionsAsQueryable(int? filterByAttributeId)
        {
            IQueryable<TblProductCheckoutAttributeOptions> result = _dbContext.ProductCheckoutAttributeOptions;
            if (filterByAttributeId != null)
            {
                result = result.Where(p => p.ProductCheckoutAttributeId == filterByAttributeId);
            }
            return result;
        }

        public virtual async Task DeleteOptionAsync(int id)
        {
            var record = await FindOptionByIdAsync(id);
            await _dbContext.ProductCheckoutAttributeOptions.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(
                typeof(TblProductCheckoutAttributeOptions).Name, id);

            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateOptionAsync(TblProductCheckoutAttributeOptions record)
        {
            var oldRecord = await FindOptionByIdAsync(record.Id);
            _dbContext.ProductCheckoutAttributeOptions.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddOptionAsync(TblProductCheckoutAttributeOptions record)
        {
            _dbContext.ProductCheckoutAttributeOptions.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.ProductCheckoutAttribute);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        #endregion
    }
}