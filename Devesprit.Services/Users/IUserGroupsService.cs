using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Users
{
    public partial interface IUserGroupsService
    {
        Task<TblUserGroups> GetHighestUserGroupAsync();
        Task<IEnumerable<TblUserGroups>> GetAsEnumerableAsync(int? fromPriority = null);
        IEnumerable<TblUserGroups> GetAsEnumerable(int? fromPriority = null);
        Task<List<SelectListItem>> GetAsSelectListAsync();
        List<SelectListItem> GetAsSelectList();
        Task<TblUserGroups> FindByIdAsync(int id);
        IQueryable<TblUserGroups> GetAsQueryable();
        Task DeleteAsync(int id);
        Task UpdateAsync(TblUserGroups record);
        Task<int> AddAsync(TblUserGroups record);
        Task<double> CalculatePlanPriceForUserAsync(int userGroupId, TblUsers user);
    }
}