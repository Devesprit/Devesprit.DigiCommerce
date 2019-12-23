using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class UserRoleModelFactory : IUserRoleModelFactory
    {
        public virtual UserRoleModel PrepareUserRoleModel(TblUserRoles userRole)
        {
            var result = userRole == null ? new UserRoleModel() : userRole.Adapt<UserRoleModel>();
            return result;
        }

        public virtual TblUserRoles PrepareTblUserRoles(UserRoleModel userRole)
        {
            var result = userRole.Adapt<TblUserRoles>();
            return result;
        }
    }
}