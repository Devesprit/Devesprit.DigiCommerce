using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Countries;
using Devesprit.Services.Users;
using Devesprit.WebFramework;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageUsers")]
    public partial class ManageUsersController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IUserGroupsService _userGroupsService;
        private readonly IUserModelFactory _userModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ICountriesService _countriesService;
        private readonly IUserRolesService _userRolesService;

        public ManageUsersController(IUsersService usersService, 
            IUserGroupsService userGroupsService, 
            IUserModelFactory userModelFactory,
            ILocalizationService localizationService,
            ICountriesService countriesService,
            IUserRolesService userRolesService)
        {
            _usersService = usersService;
            _userGroupsService = userGroupsService;
            _userModelFactory = userModelFactory;
            _localizationService = localizationService;
            _countriesService = countriesService;
            _userRolesService = userRolesService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public virtual ActionResult Grid(int? groupId)
        {
            if (groupId != null && groupId > 0)
            {
                //Filter by user group
                ViewBag.DataSource = Url.Action("GridDataSource", new { groupId = groupId });
            }
            else
            {
                ViewBag.DataSource = Url.Action("GridDataSource");
            }

            return PartialView();
        }

        [UserHasAtLeastOnePermission("ManageUsers_Add", "ManageUsers_Edit")]
        public virtual async Task<ActionResult> Editor(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var record = await UserManager.FindByIdAsync(id);
                if (record != null)
                {
                    return View(await _userModelFactory.PrepareUserModelAsync(record));
                }
            }

            return View(await _userModelFactory.PrepareUserModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageUsers_Add", "ManageUsers_Edit")]
        public virtual async Task<ActionResult> Editor(UserModel model, bool? saveAndContinue)
        {
            model.CountriesList = await _countriesService.GetAsSelectListAsync();
            model.UserGroupsList = await _userGroupsService.GetAsSelectListAsync();
            model.UserRolesList = await _userRolesService.GetAsSelectListAsync();
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Id == null)
            {
                if (!HttpContext.UserHasPermission("ManageUsers_Add"))
                {
                    return View("AccessPermissionError");
                }
            }
            else
            {
                if (!HttpContext.UserHasPermission("ManageUsers_Edit"))
                {
                    return View("AccessPermissionError");
                }
            }

            var record = _userModelFactory.PrepareTblUsers(model);
            var recordId = record.Id;
            try
            {
                if (model.Avatar != null)
                {
                    if (record.Id != null)
                    {
                        var olduser = await UserManager.FindByIdAsync(record.Id);
                        try
                        {
                            var oldAvatarFile = Server.MapPath(olduser.Avatar);
                            if (System.IO.File.Exists(oldAvatarFile))
                            {
                                System.IO.File.Delete(oldAvatarFile);
                            }
                        }
                        catch
                        { }
                    }
                    record.Avatar = model.Avatar.SaveToAppData();
                }

                if (model.Id == null)
                {
                    //Add new record
                    if (string.IsNullOrWhiteSpace(model.Password))
                    {
                        ModelState.AddModelError("Password", _localizationService.GetResource("PasswordCannotEmpty"));
                        return View(model);
                    }

                    record.RegisterDate = DateTime.Now;
                    recordId = await _usersService.AddAsync(record, model.Password);
                }
                else
                {
                    //Edit record
                    var oldUser = await UserManager.FindByIdAsync(model.Id);
                    record.RegisterDate = oldUser.RegisterDate;
                    record.UserLastLoginDate = oldUser.UserLastLoginDate;
                    record.UserLatestIP = oldUser.UserLatestIP;
                    if (model.Avatar == null)
                    {
                        record.Avatar = oldUser.Avatar;
                    }
                    await _usersService.UpdateAsync(record, model.Password);
                }

                await _usersService.SetUserRoleAsync(recordId, model.IsAdmin);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManageUsers", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshUsersGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageUsers_Delete")]
        public virtual async Task<ActionResult> Delete(Guid[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _usersService.DeleteAsync(key.ToString());
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int? groupId)
        {
            var query = _usersService.GetAsQueryable();
            if (groupId != null && groupId > 0)
            {
                query = query.Where(p => p.UserGroupId == groupId);
            }

            var strUnlimited = _localizationService.GetResource("Unlimited");
            var strPer = $" {_localizationService.GetResource("Per")} ";
            var dataSource = query.Select(p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.RegisterDate,
                p.Email,
                p.UserDisabled,
                p.UserLastLoginDate,
                p.UserLatestIP,
                GroupName = p.UserGroup.GroupName ?? "-",
                Country = p.UserCountry.CountryName ?? "-",
                p.MaxDownloadCount,
                p.MaxDownloadPeriodType,
                DownloadLimit = p.MaxDownloadCount > 0 
                    ? p.MaxDownloadCount + strPer + p.MaxDownloadPeriodType
                    : strUnlimited,
                p.SubscriptionDate,
                p.SubscriptionExpireDate
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new {result = result, count = count} : (object) result,
                JsonRequestBehavior.AllowGet);
        }
    }
}