using System;
using System.Collections.Generic;
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

namespace Devesprit.Services.Users
{
    public partial class UserGroupsService : IUserGroupsService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public UserGroupsService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<TblUserGroups> GetHighestUserGroupAsync()
        {
            var result = await _dbContext.UserGroups.OrderByDescending(p => p.GroupPriority)
                .DeferredFirstOrDefault()
                .FromCacheAsync(QueryCacheTag.UserGroup);
            return result;
        }

        public virtual async Task<IEnumerable<TblUserGroups>> GetAsEnumerableAsync(int? fromPriority = null)
        {
            var result = GetAsQueryable();
            if (fromPriority != null)
            {
                result = result.Where(p => p.GroupPriority > fromPriority.Value);
            }

            return await result.FromCacheAsync(QueryCacheTag.UserGroup);
        }

        public virtual IEnumerable<TblUserGroups> GetAsEnumerable(int? fromPriority = null)
        {
            var result = GetAsQueryable();
            if (fromPriority != null)
            {
                result = result.Where(p => p.GroupPriority > fromPriority.Value);
            }

            return result.FromCache(QueryCacheTag.UserGroup);
        }

        public virtual async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() {Value = p.Id.ToString(), Text = p.GetLocalized(x => x.GroupName)})
                .ToList();
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            var result = GetAsQueryable().FromCache(QueryCacheTag.UserGroup);

            return result.Select(p =>
                    new SelectListItem() {Value = p.Id.ToString(), Text = p.GetLocalized(x => x.GroupName)})
                .ToList();
        }

        public virtual async Task<TblUserGroups> FindByIdAsync(int id)
        {
            var result = await _dbContext.UserGroups.DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.UserGroup);
            return result;
        }

        public virtual IQueryable<TblUserGroups> GetAsQueryable()
        {
            return _dbContext.UserGroups.OrderByDescending(p => p.GroupDisplayOrder);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserGroups.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblUserGroups).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.UserGroup);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblUserGroups record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.UserGroups.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(QueryCacheTag.UserGroup);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblUserGroups record)
        {
            _dbContext.UserGroups.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(QueryCacheTag.UserGroup);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task<double> CalculatePlanPriceForUserAsync(int userGroupId, TblUsers user)
        {
            var userGroup = await FindByIdAsync(userGroupId);
            if (userGroup == null)
            {
                throw new ArgumentNullException(nameof(userGroupId));
            }

            var result = userGroup.SubscriptionFee;

            if (result <= 0)
            {
                return 0;
            }

            if (userGroup.SubscriptionDiscountPercentage > 0)
            {
                result = result - ((result * userGroup.SubscriptionDiscountPercentage) / 100);
            }

            if (user.UserGroupId != null && user.SubscriptionExpireDate > DateTime.Now && userGroup.DiscountForRenewalBeforeExpiration > 0)
            {
                result = result - ((result * userGroup.DiscountForRenewalBeforeExpiration) / 100);
            }

            return result;
        }
    }
}