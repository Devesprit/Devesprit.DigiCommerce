using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using X.PagedList;

namespace Devesprit.Services.Notifications
{
    public partial interface INotificationsService
    {
        IQueryable<TblNotifications> GetAsQueryable();

        Task DeleteAsync(int id);

        Task<TblNotifications> FindByIdAsync(int id);
        Task<TblNotifications> FindByArgumentAsync(object argument);

        Task<int> AddAsync(TblNotifications record);

        Task AddMultipleAsync(List<TblNotifications> records);

        Task SendNotificationAsync(string userId, string messageResourceName, object parameters, bool isNotification = true);

        Task SendMultipleNotificationsAsync(List<string> userIdList, string messageResourceName, object parameters, bool isNotification = true);

        Task UpdateAsync(TblNotifications record);

        int GetUserUnReadedNotificationsCount(string userId);

        Task<IPagedList<TblNotifications>> GetUserNotificationsAsPagedListAsync(string userId, bool setAsReaded, int pageIndex = 1,
            int pageSize = int.MaxValue);
    }
}
