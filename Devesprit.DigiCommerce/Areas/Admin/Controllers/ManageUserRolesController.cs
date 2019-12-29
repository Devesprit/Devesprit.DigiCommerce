using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Countries;
using Devesprit.Services.Users;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;
using Syncfusion.Linq;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageUserRoles")]
    public partial class ManageUserRolesController : BaseController
    {
        private readonly IUserRolesService _userRolesService;
        private readonly IUserRoleModelFactory _userRoleModelFactory;
        private readonly ILocalizationService _localizationService;

        public ManageUserRolesController(IUserRolesService userRolesService,
            IUserRoleModelFactory userRoleModelFactory,
            ILocalizationService localizationService)
        {
            _userRolesService = userRolesService;
            _userRoleModelFactory = userRoleModelFactory;
            _localizationService = localizationService;
        }

        public virtual ActionResult Index()
        {
            ViewBag.UserRolesList = _userRolesService.GetAsSelectList();
            return View();
        }

        [UserHasAtLeastOnePermission("ManageUserRoles_Add", "ManageUserRoles_Edit")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _userRolesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(_userRoleModelFactory.PrepareUserRoleModel(record));
                }
            }

            return View(_userRoleModelFactory.PrepareUserRoleModel(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageUserRoles_Add", "ManageUserRoles_Edit")]
        public virtual async Task<ActionResult> Editor(UserRoleModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _userRoleModelFactory.PrepareTblUserRoles(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageUserRoles_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await _userRolesService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageUserRoles_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _userRolesService.UpdateAsync(record);
                }
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshRolesDropDownBox();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageUserRoles_Delete")]
        public virtual async Task<ActionResult> Delete(int keys)
        {
            try
            {
                var role = await _userRolesService.FindByIdAsync(keys);
                if (role.RoleName == "Administrator")
                {
                    return Content(_localizationService.GetResource("YouCannotDeleteAdministratorRole"));
                }

                await _userRolesService.DeleteAsync(keys);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> GetUserRolesList()
        {
            var roles = (await _userRolesService.GetAsEnumerableAsync()).Select(p =>
                "<option value='" + p.Id + "'>" + p.RoleName + "</option>'");
            return Content("<option value=''>" + _localizationService.GetResource("SelectAnItem") + "</option>'" +
                           String.Join("", roles));
        }

        [HttpPost]
        [UserHasPermission("ManageUserRoles_ApplyPermissions")]
        public virtual async Task<ActionResult> ApplyPermissions(int roleId, int[] areas)
        {
            try
            {
                var role = await _userRolesService.FindByIdAsync(roleId);
                if (role.RoleName == "Administrator")
                {
                    return Content(_localizationService.GetResource("YouCannotChangeAdministratorPermissions"));
                }
                await _userRolesService.DeleteRolePermissionAsync(roleId);
                var accessAreas = _userRolesService.GetUserAccessAreasAsEnumerable().ToList();
                foreach (var areaId in areas)
                {
                    var area = accessAreas.FirstOrDefault(p => p.Id == areaId);
                    if (area != null)
                    {
                        await _userRolesService.AddPermissionAsync(new TblUserRolePermissions()
                        {
                            AreaName = area.AreaName,
                            HaveAccess = true,
                            RoleId = roleId
                        });
                    }
                }
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int roleId)
        {
            var rolePermissions = _userRolesService.GetAsQueryable().Where(p=> p.Id == roleId).SelectMany(p=> p.Permissions).ToList();
            var accessAreas = _userRolesService.GetUserAccessAreasAsEnumerable().ToList();

            var result = accessAreas.OrderBy(p => p.AreaName).Select(p => new
            { 
                p.Id,
                p.ParentAreaName,
                p.AreaName,
                ParentAreaId = accessAreas.FirstOrDefault(x=> x.AreaName == p.ParentAreaName)?.Id,
                AreaNameLocalized = _localizationService.GetResource(p.AreaNameLocalizationResource),
                HaveAccess = rolePermissions.FirstOrDefault(x=> x.AreaName == p.AreaName)?.HaveAccess ?? false
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}