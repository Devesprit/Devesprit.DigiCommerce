using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.Services.Events;
using Devesprit.Services.Notifications;
using Devesprit.Services.Products;
using Devesprit.Services.Users.Events;
using Devesprit.Utilities.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using X.PagedList;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Users
{
    public partial class UsersService : IUsersService
    {
        #region Fields

        private readonly AppDbContext _dbContext;
        private readonly IUserLikesService _userLikesService;
        private readonly IUserWishlistService _userWishlistService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;
        private readonly INotificationsService _notificationsService;
        private readonly ISettingService _settingService;
        private readonly IEventPublisher _eventPublisher;
        private UserManager<TblUsers> _userManager;

        #endregion

        #region Ctor

        public UsersService(AppDbContext dbContext,
            IUserLikesService userLikesService,
            IUserWishlistService userWishlistService,
            IProductCheckoutAttributesService productCheckoutAttributesService,
            INotificationsService notificationsService,
            ISettingService settingService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _userLikesService = userLikesService;
            _userWishlistService = userWishlistService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
            _notificationsService = notificationsService;
            _settingService = settingService;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region CURD Methods

        public virtual IQueryable<TblUsers> GetAsQueryable()
        {
            return _dbContext.Users.OrderBy(p => p.RegisterDate);
        }

        public virtual async Task UpdateAsync(TblUsers record, string password)
        {
            if (await _dbContext.Users.AnyAsync(p => p.Email.Trim() == record.Email.Trim() && p.Id != record.Id))
            {
                throw new Exception($"The \"{record.Email}\" email address already exists.");
            }

            var oldRecord = await UserManager.FindByIdAsync(record.Id);

            if (oldRecord != null)
            {
                record.UserName = record.Email;
                if (string.IsNullOrWhiteSpace(record.Avatar))
                {
                    record.Avatar = oldRecord.Avatar;
                }

                if (string.IsNullOrEmpty(password))
                {
                    record.PasswordHash = oldRecord.PasswordHash;
                }
                else
                {
                    var passwordValidatorResult = await UserManager.PasswordValidator.ValidateAsync(password);
                    if (!passwordValidatorResult.Succeeded)
                    {
                        throw new Exception(passwordValidatorResult.Errors.StringJoin(Environment.NewLine));
                    }
                    record.PasswordHash = HashPassword(password);
                }

                record.SecurityStamp = oldRecord.SecurityStamp;

                _dbContext.Users.AddOrUpdate(record);
                await _dbContext.SaveChangesAsync();

                _eventPublisher.EntityUpdated(record, oldRecord);
            }
        }

        public virtual async Task<string> AddAsync(TblUsers record, string password)
        {
            if (await UserManager.FindByEmailAsync(record.Email) != null)
            {
                throw new Exception($"The \"{record.Email}\" email address already exists.");
            }

            if (string.IsNullOrWhiteSpace(record.UserName))
            {
                record.UserName = record.Email;
            }

            var result = await UserManager.CreateAsync(record, password);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.StringJoin(Environment.NewLine));
            }

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task DeleteAsync(string id)
        {
            var record = await UserManager.FindByIdAsync(id);
            await _dbContext.Users.Where(p=> p.Id == id).DeleteAsync();

            _eventPublisher.EntityDeleted(record);
        }

        #endregion

        public virtual UserManager<TblUsers> UserManager
        {
            get
            {
                if (_userManager != null)
                {
                    return _userManager;
                }

                var settings = _settingService.LoadSetting<SiteSettings>();
                _userManager = new UserManager<TblUsers>(new UserStore<TblUsers>(_dbContext));
                _userManager.UserValidator = new UserValidator<TblUsers>(_userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };
                _userManager.PasswordValidator = new CustomIdentityPasswordValidator
                {
                    RequiredLength = settings.PasswordRequiredLength,
                    RequireNonLetterOrDigit = settings.PasswordRequireNonLetterOrDigit,
                    RequireDigit = settings.PasswordRequireDigit,
                    RequireLowercase = settings.PasswordRequireLowercase,
                    RequireUppercase = settings.PasswordRequireUppercase,
                };

                _userManager.UserLockoutEnabledByDefault = settings.UserLockoutEnabled;
                _userManager.DefaultAccountLockoutTimeSpan = settings.AccountLockoutTimeSpan;
                _userManager.MaxFailedAccessAttemptsBeforeLockout = settings.MaxFailedAccessAttemptsBeforeLockout;
                return _userManager;
            }
        }

        public void SetUserLatestIpAndLoginDate(string id, string ip)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                var user = _dbContext.Users.FirstOrDefault(p => p.Id == id);
                if (user != null)
                {
                    user.UserLastLoginDate = DateTime.Now;
                    user.UserLatestIP = ip;

                    _dbContext.SaveChanges();
                }
            }
        }

        public virtual async Task SetUserRoleAsync(string userId, bool isAdmin)
        {
            IdentityResult result = null;
            if (isAdmin)
            {
                if (!await UserIsAdminAsync(userId))
                    result = await UserManager.AddToRoleAsync(userId, "Admin");
            }
            else
            {
                if (await UserIsAdminAsync(userId))
                    result = await UserManager.RemoveFromRoleAsync(userId, "Admin");
            }

            if (result != null && !result.Succeeded)
            {
                throw new Exception(result.Errors.StringJoin(Environment.NewLine));
            }

            _eventPublisher.Publish(new UserRoleChangeEvent(userId, isAdmin));
        }

        public virtual async Task<bool> UserIsAdminAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            try
            {
                return await UserManager.IsInRoleAsync(userId, "Admin");
            }
            catch
            {
                return false;
            }
        }

        public bool UserIsAdmin(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            try
            {
                return  UserManager.IsInRole(userId, "Admin");
            }
            catch
            {
                return false;
            }
        }

        public virtual List<UserPurchasedProduct> GetUserPurchasedProducts(string userId, int? filterByProductId)
        {
            var userInvoices = _dbContext.Invoices.Where(p => p.UserId == userId && p.Status == InvoiceStatus.Paid)
                .Include(p => p.InvoiceDetails).Include(p => p.User).AsNoTracking().FromCache(QueryCacheTag.Invoice);

            var result = new List<UserPurchasedProduct>();
            foreach (var invoice in userInvoices)
            {
                var purchaseDate = invoice.PaymentDate ?? invoice.CreateDate;
                foreach (var detail in invoice.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.Product))
                {
                    if (filterByProductId != null && detail.ItemId != filterByProductId) continue;

                    var item = result.FirstOrDefault(p => p.ProductId == detail.ItemId);
                    if (item != null)//already Added
                    {
                        if (item.PurchaseDate < purchaseDate)
                        {
                            item.InvoiceDetailsId = detail.Id;
                            item.InvoiceId = detail.InvoiceId;
                            item.PurchaseDate = purchaseDate;
                            item.PurchaseExpiration = detail.PurchaseExpiration ?? DateTime.MaxValue;
                            item.ProductHomePage = detail.ItemHomePage;
                            item.ProductName = detail.ItemName;
                            item.User = invoice.User;
                        }
                    }
                    else
                    {
                        result.Add(new UserPurchasedProduct()
                        {
                            InvoiceDetailsId = detail.Id,
                            InvoiceId = detail.InvoiceId,
                            ProductId = detail.ItemId,
                            PurchaseDate = purchaseDate,
                            PurchaseExpiration = detail.PurchaseExpiration ?? DateTime.MaxValue,
                            ProductHomePage = detail.ItemHomePage,
                            ProductName = detail.ItemName,
                            User = invoice.User
                        });
                    }
                }
            }

            return result;
        }

        public virtual async Task<List<UserPurchasedProductAttribute>> GetUserPurchasedProductAttributesAsync(string userId, int? filterByProductId)
        {
            var userInvoices = await _dbContext.Invoices
                .Where(p => p.UserId == userId && p.Status == InvoiceStatus.Paid)
                .Include(p => p.InvoiceDetails).Include(p => p.User).AsNoTracking().FromCacheAsync(QueryCacheTag.Invoice);

            var result = new List<UserPurchasedProductAttribute>();
            foreach (var invoice in userInvoices)
            {
                var purchaseDate = invoice.PaymentDate ?? invoice.CreateDate;
                foreach (var detail in invoice.InvoiceDetails.Where(p => p.ItemType == InvoiceDetailsItemType.ProductAttributeOption))
                {
                    var option = await _productCheckoutAttributesService.FindOptionByIdAsync(
                        detail.ItemId);

                    if (option == null)
                    {
                        continue;
                    }

                    if (filterByProductId != null &&
                        option.ProductCheckoutAttribute.ProductId != filterByProductId) continue;

                    var item = result.FirstOrDefault(p => p.Option.Id == detail.ItemId);
                    if (item != null)//already Added
                    {
                        if (item.PurchaseDate < purchaseDate)
                        {
                            item.PurchaseDate = purchaseDate;
                            item.PurchaseExpiration = detail.PurchaseExpiration ?? DateTime.MaxValue;
                            item.User = invoice.User;
                            item.InvoiceId = invoice.Id;
                            item.InvoiceDetailsId = detail.Id;
                            item.HomePage = detail.ItemHomePage;
                            item.Name = detail.ItemName;
                            item.AttributeOptionId = option.Id;
                            item.AttributeId = option.ProductCheckoutAttributeId;
                        }
                    }
                    else
                    {
                        result.Add(new UserPurchasedProductAttribute()
                        {
                            Option = option,
                            PurchaseDate = purchaseDate,
                            PurchaseExpiration = detail.PurchaseExpiration ?? DateTime.MaxValue,
                            User = invoice.User,
                            InvoiceId = invoice.Id,
                            InvoiceDetailsId = detail.Id,
                            HomePage = detail.ItemHomePage,
                            Name = detail.ItemName,
                            AttributeOptionId = option.Id,
                            AttributeId = option.ProductCheckoutAttributeId
                        });
                    }
                }
            }

            return result;
        }
        
        public virtual async Task<IPagedList<TblProductDownloadsLog>> GetUserDownloadsLogAsync(string userId, int pageIndex = 1, int pageSize = 10)
        {
            var query = from downloads in _dbContext.ProductDownloadsLog
                where downloads.UserId == userId
                group downloads by downloads.ProductId
                into grp
                select grp.OrderByDescending(x => x.DownloadDate).FirstOrDefault();

            var result = new StaticPagedList<TblProductDownloadsLog>(
                await query
                    .OrderByDescending(p=> p.DownloadDate)
                    .Include(p => p.Product)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.ProductDownloadLog),
                pageIndex,
                pageSize,
                await query
                    .DeferredCount(p => p.UserId == userId)
                    .FromCacheAsync(QueryCacheTag.ProductDownloadLog));

            return result;
        }

        public virtual async Task<IPagedList<TblUserWishlist>> GetUserWishlistAsync(string userId, int pageIndex = 1, int pageSize = 10)
        {
            var result = new StaticPagedList<TblUserWishlist>(
                await _userWishlistService.GetAsQueryable()
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.Date)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.UserWishlist),
                pageIndex,
                pageSize,
                await _dbContext.UserWishlist
                    .DeferredCount(p => p.UserId == userId)
                    .FromCacheAsync(QueryCacheTag.UserWishlist));

            return result;
        }

        public virtual async Task<IPagedList<TblUserLikes>> GetUserLikedPostsAsync(string userId, int pageIndex = 1, int pageSize = 10)
        {
            var result = new StaticPagedList<TblUserLikes>(
                await _userLikesService.GetAsQueryable()
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.Date)
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.UserLikes),
                pageIndex,
                pageSize,
                await _dbContext.UserLikes
                    .DeferredCount(p => p.UserId == userId)
                    .FromCacheAsync(QueryCacheTag.UserLikes));

            return result;
        }

        public virtual async Task<IPagedList<TblInvoices>> GetUserInvoicesAsync(string userId, int pageIndex = 1, int pageSize = 10, bool hideUnpaidInvoices = true)
        {
            var query = _dbContext.Invoices
                .Where(p => p.UserId == userId);

            if (hideUnpaidInvoices)
            {
                query = query.Where(p => !(p.Status == InvoiceStatus.Pending && p.InvoiceDetails.Count == 0));
            }

            var result = new StaticPagedList<TblInvoices>(
                await query.OrderByDescending(p => p.CreateDate)
                    .Include(p => p.InvoiceDetails)
                    .AsNoTracking()
                    .Skip(pageSize * (pageIndex - 1))
                    .Take(pageSize)
                    .FromCacheAsync(QueryCacheTag.Invoice),
                pageIndex,
                pageSize,
                await _dbContext.Invoices
                    .DeferredCount(p => p.UserId == userId)
                    .FromCacheAsync(QueryCacheTag.Invoice));

            return result;
        }

        public virtual async Task UpgradeCustomerUserGroupAsync(string userId, TblUserGroups userGroup, DateTime? startDate = null)
        {
            if (startDate == null)
            {
                startDate = DateTime.Now;
            }

            var subscriptionExpireDate = startDate?.AddTimePeriodToDateTime(userGroup.SubscriptionExpirationPeriodType,
                userGroup.SubscriptionExpirationTime);

            await _dbContext.Users.Where(p => p.Id == userId).UpdateAsync(p=> new TblUsers()
            {
                SubscriptionDate = startDate,
                SubscriptionExpireDate = subscriptionExpireDate,
                UserGroupId = userGroup.Id
            });

            _eventPublisher.Publish(new CustomerUserGroupChangeEvent(userId, userGroup, startDate));
        }

        public virtual async Task SendExpirationNotificationsAsync()
        {
            var currentSettings = await _settingService.LoadSettingAsync<SiteSettings>();
            //Send Product & Product Attributes Options Expiration Notifications
            if (currentSettings.SendExpNotificationsForProducts || currentSettings.SendExpNotificationsForProductAttributes)
            {
                var fromDate = DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForProducts);
                var toDate = DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForProducts);
                var usersList = await _dbContext.Invoices
                   .Where(p => p.User != null && p.Status == InvoiceStatus.Paid)
                   .SelectMany(p => p.InvoiceDetails)
                   .Where(p =>
                       (p.ItemType == InvoiceDetailsItemType.Product ||
                        p.ItemType == InvoiceDetailsItemType.ProductAttributeOption) &&
                       p.PurchaseExpiration != null &&
                       p.PurchaseExpiration >= fromDate &&
                       p.PurchaseExpiration <= toDate)
                   .Include(p => p.Invoice).Select(p => p.Invoice.UserId).FromCacheAsync(QueryCacheTag.Invoice);

                foreach (var user in usersList.Distinct())
                {

                    //Send Product Expiration Notifications
                    if (currentSettings.SendExpNotificationsForProducts)
                    {
                        foreach (var product in GetUserPurchasedProducts(user, null))
                        {
                            var argument = new
                            {
                                UserFullName = $"{product.User.FirstName} {product.User.LastName}",
                                UserEmail = product.User.Email,
                                UserId = user,
                                ItemName = product.ProductName,
                                ItemType = InvoiceDetailsItemType.Product,
                                ItemHomePage = product.ProductHomePage,
                                ItemId = product.ProductId,
                                PurchaseDate = product.PurchaseDate.ToString("F"),
                                ExpirationDate = product.PurchaseExpiration.ToString("F"),
                                InvoiceId = product.InvoiceId,
                                InvoiceDetailId = product.InvoiceDetailsId,
                            };

                            if (currentSettings.SendExpNotificationsForProductsJustOnce)
                            {
                                if (await _notificationsService.FindByArgumentAsync(argument) != null)
                                {
                                    //already notification was sent
                                    continue;
                                }
                            }

                            if (product.PurchaseExpiration >= DateTime.Now &&
                                product.PurchaseExpiration <= DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForProducts))
                            {
                                await _notificationsService.SendNotificationAsync(user, "Notification_ProductBeforeExpiration", argument);
                            }
                            else if (product.PurchaseExpiration <= DateTime.Now &&
                                     product.PurchaseExpiration >= DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForProducts))
                            {
                                await _notificationsService.SendNotificationAsync(user, "Notification_ProductAfterExpiration", argument);
                            }
                        }
                    }


                    //Send Product Attributes Options Expiration Notifications
                    if (currentSettings.SendExpNotificationsForProductAttributes)
                    {
                        foreach (var attrOption in await GetUserPurchasedProductAttributesAsync(user, null))
                        {
                            var argument = new
                            {
                                UserFullName = $"{attrOption.User.FirstName} {attrOption.User.LastName}",
                                UserEmail = attrOption.User.Email,
                                UserId = user,
                                ItemName = attrOption.Name,
                                ItemType = InvoiceDetailsItemType.ProductAttributeOption,
                                ItemHomePage = attrOption.HomePage,
                                ItemId = attrOption.AttributeOptionId,
                                PurchaseDate = attrOption.PurchaseDate.ToString("F"),
                                ExpirationDate = attrOption.PurchaseExpiration.ToString("F"),
                                InvoiceId = attrOption.InvoiceId,
                                InvoiceDetailId = attrOption.InvoiceDetailsId,
                            };

                            if (currentSettings.SendExpNotificationsForProductAttributesJustOnce)
                            {
                                if (await _notificationsService.FindByArgumentAsync(argument) != null)
                                {
                                    //already notification was sent
                                    continue;
                                }
                            }

                            if (attrOption.PurchaseExpiration >= DateTime.Now &&
                                attrOption.PurchaseExpiration <= DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForProducts))
                            {
                                await _notificationsService.SendNotificationAsync(user, "Notification_ProductAttributeBeforeExpiration", argument);
                            }
                            else if (attrOption.PurchaseExpiration <= DateTime.Now &&
                                     attrOption.PurchaseExpiration >= DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForProducts))
                            {
                                await _notificationsService.SendNotificationAsync(user, "Notification_ProductAttributeAfterExpiration", argument);
                            }
                        }
                    }
                }
            }


            //Send User Account Plan Expiration Notifications
            if (currentSettings.SendExpNotificationsForUserPlans)
            {
                var fromDate = DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForUserPlans);
                var toDate = DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForUserPlans);
                var users = await _dbContext.Users.Where(p =>
                    p.UserGroupId != null && p.SubscriptionExpireDate != null &&
                    p.SubscriptionExpireDate >=  fromDate &&
                    p.SubscriptionExpireDate <= toDate)
                .Include(p => p.UserGroup).ToListAsync();
                
                foreach (var user in users)
                {
                    var argument = new
                    {
                        UserFullName = $"{user.FirstName} {user.LastName}",
                        UserEmail = user.Email,
                        UserId = user.Id,
                        ItemName = user.UserGroup.GroupName,
                        ItemType = InvoiceDetailsItemType.SubscriptionPlan.ToString(),
                        ItemHomePage = currentSettings.SiteUrl.TrimEnd("/")+"/Purchase/UpgradeAccount",
                        ItemId = user.UserGroupId,
                        ExpirationDate = user.SubscriptionExpireDate.Value.ToString("F"),
                    };

                    if (currentSettings.SendExpNotificationsForUserPlansJustOnce)
                    {
                        if (await _notificationsService.FindByArgumentAsync(argument) != null)
                        {
                            //already notification was sent
                            continue;
                        }
                    }

                    if (user.SubscriptionExpireDate >= DateTime.Now &&
                        user.SubscriptionExpireDate <= DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForUserPlans))
                    {
                        await _notificationsService.SendNotificationAsync(user.Id, "Notification_UserPlanBeforeExpiration", argument);
                    }
                    else if (user.SubscriptionExpireDate <= DateTime.Now &&
                             user.SubscriptionExpireDate >= DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForUserPlans))
                    {
                        await _notificationsService.SendNotificationAsync(user.Id, "Notification_UserPlanAfterExpiration", argument);
                    }
                }
            }


            //Send Other Expiration Notifications
            if (currentSettings.SendExpNotificationsForInvoiceManualItems)
            {
                var fromDate = DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForInvoiceManualItems);
                var toDate = DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForInvoiceManualItems);
                var invoiceDetails = await _dbContext.Invoices
                   .Where(p => p.User != null && p.Status == InvoiceStatus.Paid)
                   .SelectMany(p => p.InvoiceDetails)
                   .Where(p => p.ItemType == InvoiceDetailsItemType.Other &&
                               p.PurchaseExpiration != null &&
                               p.PurchaseExpiration >= fromDate &&
                               p.PurchaseExpiration <= toDate)
                   .Include(p => p.Invoice).Include(p => p.Invoice.User).FromCacheAsync(QueryCacheTag.Invoice);

                foreach (var invoiceDetail in invoiceDetails)
                {
                    var argument = new
                    {
                        UserFullName = $"{invoiceDetail.Invoice.User.FirstName} {invoiceDetail.Invoice.User.LastName}",
                        UserEmail = invoiceDetail.Invoice.User.Email,
                        UserId = invoiceDetail.Invoice.UserId,
                        ItemName = invoiceDetail.ItemName,
                        ItemType = InvoiceDetailsItemType.Other,
                        ItemHomePage = invoiceDetail.ItemHomePage,
                        ItemId = invoiceDetail.ItemId,
                        PurchaseDate = (invoiceDetail.Invoice.PaymentDate ?? invoiceDetail.Invoice.CreateDate).ToString("F"),
                        ExpirationDate = invoiceDetail.PurchaseExpiration.Value.ToString("F"),
                        InvoiceId = invoiceDetail.InvoiceId,
                        InvoiceDetailId = invoiceDetail.Id,
                    };

                    if (currentSettings.SendExpNotificationsForInvoiceManualItemsJustOnce)
                    {
                        if (await _notificationsService.FindByArgumentAsync(argument) != null)
                        {
                            //already notification was sent
                            continue;
                        }
                    }

                    if (invoiceDetail.PurchaseExpiration >= DateTime.Now &&
                        invoiceDetail.PurchaseExpiration <= DateTime.Now.AddHours(currentSettings.NotificationHourSpanBeforeExpForInvoiceManualItems))
                    {
                        await _notificationsService.SendNotificationAsync(invoiceDetail.Invoice.UserId, "Notification_InvoiceManuallyBeforeExpiration", argument);
                    }
                    else if (invoiceDetail.PurchaseExpiration <= DateTime.Now &&
                             invoiceDetail.PurchaseExpiration >= DateTime.Now.AddHours(-currentSettings.NotificationHourSpanAfterExpForInvoiceManualItems))
                    {
                        await _notificationsService.SendNotificationAsync(invoiceDetail.Invoice.UserId, "Notification_InvoiceManuallyAfterExpiration", argument);
                    }
                }
            }
        }

        public virtual async Task<Dictionary<DateTime, int>> UsersReportAsync(DateTime fromDate, DateTime toDate, TimePeriodType periodType, bool emailVerifiedUsersOnly, int userGroupId)
        {
            var query = _dbContext.Users.Where(p => p.RegisterDate >= fromDate && p.RegisterDate <= toDate);
            if (emailVerifiedUsersOnly)
            {
                query = query.Where(p => p.EmailConfirmed);
            }

            if (userGroupId > 0)
            {
                query = query.Where(p => p.UserGroupId == userGroupId);
            }

            var users = await query.OrderBy(p => p.RegisterDate).Select(p => new { p.RegisterDate }).ToListAsync();

            var report = new Dictionary<DateTime, int>();
            var datetimeToStringFormat = "g";
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    datetimeToStringFormat = "yyyy/MM/dd HH:mm";
                    report = users.GroupBy(p =>
                            new DateTime(p.RegisterDate.Year, p.RegisterDate.Month, p.RegisterDate.Day, p.RegisterDate.Hour, 0,
                                0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Day:
                    datetimeToStringFormat = "yyyy/MM/dd";
                    report = users.GroupBy(p =>
                            new DateTime(p.RegisterDate.Year, p.RegisterDate.Month, p.RegisterDate.Day, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Month:
                    datetimeToStringFormat = "yyyy/MM";
                    report = users.GroupBy(p =>
                            new DateTime(p.RegisterDate.Year, p.RegisterDate.Month, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
                case TimePeriodType.Year:
                    datetimeToStringFormat = "yyyy";
                    report = users.GroupBy(p =>
                            new DateTime(p.RegisterDate.Year, 1, 1, 0, 0, 0, 0))
                        .Select(p => new { date = p.Key, count = p.Count() })
                        .ToDictionary(p => p.date, p => p.count);
                    break;
            }

            //Insert Zero Values
            var dateCounter = fromDate;
            while (dateCounter < toDate)
            {
                if (report.All(p => p.Key.ToString(datetimeToStringFormat) != dateCounter.ToString(datetimeToStringFormat)))
                {
                    report.Add(dateCounter, 0);
                }
                switch (periodType)
                {
                    case TimePeriodType.Hour:
                        dateCounter = dateCounter.AddHours(1);
                        break;
                    case TimePeriodType.Day:
                        dateCounter = dateCounter.AddDays(1);
                        break;
                    case TimePeriodType.Month:
                        dateCounter = dateCounter.AddMonths(1);
                        break;
                    case TimePeriodType.Year:
                        dateCounter = dateCounter.AddYears(1);
                        break;
                }
            }

            return report.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        }



        #region Password Hash Generator
        protected virtual string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

        protected virtual bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return ByteArraysEqual(buffer3, buffer4);
        }

        protected virtual bool ByteArraysEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;
            if (b1 == null || b2 == null) return false;
            if (b1.Length != b2.Length) return false;
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i]) return false;
            }
            return true;
        }
        #endregion
    }
}
