using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Microsoft.AspNet.Identity;
using X.PagedList;

namespace Devesprit.Services.Users
{
    public partial interface IUsersService
    {
        UserManager<TblUsers> UserManager { get; }
        IQueryable<TblUsers> GetAsQueryable();
        Task UpdateAsync(TblUsers record, string password);
        Task<string> AddAsync(TblUsers record, string password);
        Task DeleteAsync(string id);
        void SetUserLatestIpAndLoginDate(string id, string ip);
        Task SetUserRoleAsync(string userId, bool isAdmin);
        Task<bool> UserIsAdminAsync(string userId);
        bool UserIsAdmin(string userId);
        List<UserPurchasedProduct> GetUserPurchasedProducts(string userId, int? filterByProductId);
        Task<List<UserPurchasedProductAttribute>> GetUserPurchasedProductAttributesAsync(string userId, int? filterByProductId);
        Task<IPagedList<TblProductDownloadsLog>> GetUserDownloadsLogAsync(string userId, int pageIndex = 1, int pageSize = 10);
        Task<IPagedList<TblUserWishlist>> GetUserWishlistAsync(string userId, int pageIndex = 1, int pageSize = 10);
        Task<IPagedList<TblUserLikes>> GetUserLikedPostsAsync(string userId, int pageIndex = 1, int pageSize = 10);
        Task<IPagedList<TblInvoices>> GetUserInvoicesAsync(string userId, int pageIndex = 1, int pageSize = 10, bool hideUnpaidInvoices = true);
        Task UpgradeCustomerUserGroupAsync(string userId, TblUserGroups userGroup, DateTime? startDate = null);
        Task SendExpirationNotificationsAsync();
        Task<Dictionary<DateTime, int>> UsersReportAsync(DateTime fromDate, DateTime toDate,
            TimePeriodType periodType, bool emailVerifiedUsersOnly, int userGroupId);
    }
}
