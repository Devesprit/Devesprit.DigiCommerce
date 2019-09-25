using System.Threading.Tasks;
using System.Web;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Countries;
using Devesprit.Services.Users;
using Mapster;
using Microsoft.AspNet.Identity.Owin;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class UserModelFactory : IUserModelFactory
    {
        private readonly ICountriesService _countriesService;
        private readonly IUserGroupsService _userGroupsService;

        public UserModelFactory(ICountriesService countriesService, IUserGroupsService userGroupsService)
        {
            _countriesService = countriesService;
            _userGroupsService = userGroupsService;
        }

        public virtual async Task<UserModel> PrepareUserModelAsync(TblUsers user)
        {
            var result = user == null ? new UserModel() : user.Adapt<UserModel>();
            if (user != null)
            {
                result.CurrentAvatarUrl = user.Avatar;
                result.IsAdmin = await HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
                    .IsInRoleAsync(user.Id, "Admin");
            }
            result.CountriesList = await _countriesService.GetAsSelectListAsync(); 
            result.UserGroupsList = await _userGroupsService.GetAsSelectListAsync();
            return result;
        }

        public virtual TblUsers PrepareTblUsers(UserModel user)
        {
            var result = user.Adapt<TblUsers>();
            return result;
        }
    }
}