using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Users
{
    public partial interface IUserRolesService
    {
        Task<IEnumerable<TblUserRoles>> GetAsEnumerableAsync();
        IEnumerable<TblUserRoles> GetAsEnumerable();
        Task<List<SelectListItem>> GetAsSelectListAsync();
        List<SelectListItem> GetAsSelectList();
        Task<TblUserRoles> FindByIdAsync(int id);
        TblUserRoles FindById(int id);
        IQueryable<TblUserRoles> GetAsQueryable();
        Task DeleteAsync(int id);
        Task UpdateAsync(TblUserRoles record);
        Task<int> AddAsync(TblUserRoles record);
        Task<TblUserRolePermissions> FindPermissionByIdAsync(int id);
        Task DeleteRolePermissionAsync(int roleId);
        Task DeletePermissionAsync(int id);
        Task UpdatePermissionAsync(TblUserRolePermissions record);
        Task<int> AddPermissionAsync(TblUserRolePermissions record);
        IQueryable<TblUserAccessAreas> GetUserAccessAreasAsQueryable();
        Task<IEnumerable<TblUserAccessAreas>> GetUserAccessAreasAsEnumerableAsync();
        IEnumerable<TblUserAccessAreas> GetUserAccessAreasAsEnumerable();
        Task<TblUserAccessAreas> FindAccessAreasByNameAsync(string name);
        TblUserAccessAreas FindAccessAreasByName(string name);
        Task<int> AddAccessAreasAsync(TblUserAccessAreas record);
        Task DeleteAccessAreasAsync(int id);
        Task UpdateAccessAreasAsync(TblUserAccessAreas record);
        Task GrantAllPermissionsToAdministrator();
    }
}