using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.EMail;
using Devesprit.Services.Events;
using Devesprit.Services.Notifications;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Users
{
    public partial class UserMessagingService : IUserMessagingService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly HttpContextBase _httpContext;
        private readonly INotificationsService _notificationsService;
        private readonly IEmailService _emailService;

        public UserMessagingService(AppDbContext dbContext,
            IEventPublisher eventPublisher,
            ISettingService settingService,
            IWorkContext workContext,
            HttpContextBase httpContext,
            INotificationsService notificationsService,
            IEmailService emailService)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _settingService = settingService;
            _workContext = workContext;
            _httpContext = httpContext;
            _notificationsService = notificationsService;
            _emailService = emailService;
        }

        public virtual IQueryable<TblUserMessages> GetAsQueryable()
        {
            return _dbContext.UserMessages.OrderByDescending(p => p.ReceiveDate);
        }

        public virtual async Task<TblUserMessages> FindByIdAsync(int id)
        {
            return await _dbContext.UserMessages
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.UserMessages);
        }

        public virtual async Task UpdateAsync(TblUserMessages record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.UserMessages.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserMessages);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblUserMessages record)
        {
            _dbContext.UserMessages.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserMessages);

            _eventPublisher.EntityInserted(record);

            //Send Notification to admin
            var urlHelper = new UrlHelper(_httpContext.Request.RequestContext);
            var messageUrl = urlHelper.Action("ReplyToUserMessage", "ManageUserMessages", new {id = record.Id, area = "Admin"},
                _httpContext.Request.Url.Scheme);

            if (!_workContext.IsAdmin)
            {
                //Notification to admin
                await _notificationsService.SendNotificationAsync("Admin",
                    "Notification_NewUserMessage",
                    new
                    {
                        Name = record.Name,
                        Email = record.Email,
                        Url = messageUrl
                    });
            }
            //--------------

            return record.Id;
        }

        public virtual async Task<int> NumberOfUreadedMessages()
        {
            return await _dbContext.UserMessages.DeferredCount(p => p.Readed == false)
                .FromCacheAsync(CacheTags.UserMessages);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserMessages.Where(p => p.Id == id).DeleteAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserMessages);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task ReplyToMessage(int id, string text)
        {
            var message = await FindByIdAsync(id);
            message.ReplyDate = DateTime.Now;
            message.Replied = true;
            message.ResponseText = text;
            await UpdateAsync(message);

            var currentSettings = await _settingService.LoadSettingAsync<SiteSettings>();

            await _emailService.SendEmailAsync(text,
                $"{currentSettings.SiteName[0]} - {message.Subject}",
                message.Email);
        }

        public virtual async Task SetAsReaded(int id)
        {
            var message = await FindByIdAsync(id);
            message.Readed = true;
            await UpdateAsync(message);
        }

        public virtual async Task SetAsUnReaded(int id)
        {
            var message = await FindByIdAsync(id);
            message.Readed = false;
            await UpdateAsync(message);
        }
    }
}
