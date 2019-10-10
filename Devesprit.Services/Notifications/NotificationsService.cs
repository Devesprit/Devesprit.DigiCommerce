using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Utilities.Extensions;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Notifications
{
    public partial class NotificationsService : INotificationsService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public NotificationsService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblNotifications> GetAsQueryable()
        {
            return _dbContext.Notifications.OrderByDescending(p => p.NotificationDate);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.Notifications.Where(p=> p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.Notification);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<TblNotifications> FindByIdAsync(int id)
        {
            var result = await _dbContext.Notifications
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.Notification);
            return result;
        }

        public virtual async Task<TblNotifications> FindByArgumentAsync(object argument)
        {
            var argumentStr = argument.ObjectToJson();
            var result = await _dbContext.Notifications
                .DeferredFirstOrDefault(p => p.MessageArguments == argumentStr)
                .FromCacheAsync(CacheTags.Notification);
            return result;
        }

        public virtual async Task<int> AddAsync(TblNotifications record)
        {
            _dbContext.Notifications.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.Notification);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task AddMultipleAsync(List<TblNotifications> records)
        {
            if (records.Any())
            {
                _dbContext.Notifications.AddRange(records);
                await _dbContext.SaveChangesAsync();

                QueryCacheManager.ExpireTag(CacheTags.Notification);

                records.ForEach(r => _eventPublisher.EntityInserted(r));
            }
        }

        public async Task SendNotificationAsync(string userId, string messageResourceName, object parameters, bool isNotification = true)
        {
            await AddAsync(new TblNotifications()
            {
                NotificationDate = DateTime.Now,
                Readed = false,
                UserId = userId.EqualsIgnoreCase("Admin") || string.IsNullOrWhiteSpace(userId) ? null : userId,
                MessageResourceName = messageResourceName,
                MessageArguments = parameters.ObjectToJson(),
                IsMessage = !isNotification
            });
        }

        public async Task SendMultipleNotificationsAsync(List<string> userIdList, string messageResourceName, object parameters, bool isNotification = true)
        {
            await AddMultipleAsync(userIdList.Select(p => new TblNotifications()
            {
                NotificationDate = DateTime.Now,
                Readed = false,
                UserId = p.EqualsIgnoreCase("Admin") || string.IsNullOrWhiteSpace(p) ? null : p,
                MessageResourceName = messageResourceName,
                MessageArguments = parameters.ObjectToJson(),
                IsMessage = !isNotification
            }).ToList());
        }

        public virtual async Task UpdateAsync(TblNotifications record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.Notifications.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.Notification);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual int GetUserUnReadedNotificationsCount(string userId)
        {
            return _dbContext.Notifications
                .DeferredCount(p => p.UserId == userId && !p.Readed)
                .FromCache(CacheTags.Notification);
        }

        public virtual async Task<IPagedList<TblNotifications>> GetUserNotificationsAsPagedListAsync(string userId, bool setAsReaded, int pageIndex = 1, int pageSize = int.MaxValue)
        {
            var query = GetAsQueryable();
            query = query.Where(p => p.UserId == userId);
            
            var result = new StaticPagedList<TblNotifications>(
                await query
                    .OrderByDescending(p => p.NotificationDate)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(CacheTags.Notification),
                pageIndex,
                pageSize,
                await query
                    .DeferredCount()
                    .FromCacheAsync(CacheTags.Notification));

            if (setAsReaded)
            {
                var selectedIds = result.Select(p => p.Id).ToList();
                await _dbContext.Notifications.Where(p => selectedIds.Contains(p.Id))
                    .UpdateAsync(p => new TblNotifications() { Readed = true });
                QueryCacheManager.ExpireTag(CacheTags.Notification);
            }

            return result;
        }
    }
}