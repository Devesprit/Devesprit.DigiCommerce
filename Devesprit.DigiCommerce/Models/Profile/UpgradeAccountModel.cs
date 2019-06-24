using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Services.Users;

namespace Devesprit.DigiCommerce.Models.Profile
{
    public partial class UpgradeAccountModel
    {
        public List<TblUserGroups> UserGroupsList =>
            DependencyResolver.Current.GetService<IUserGroupsService>().GetAsEnumerable().ToList();
        public TblUsers CurrentUser { get; set; }
    }
}