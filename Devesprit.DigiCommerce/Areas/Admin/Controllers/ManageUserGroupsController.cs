using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Users;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageUserGroups")]
    public partial class ManageUserGroupsController : BaseController
    {
        private readonly IUserGroupsService _userGroupsService;
        private readonly IUserGroupsModelFactory _userGroupsModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManageUserGroupsController(IUserGroupsService userGroupsService, 
            IUserGroupsModelFactory userGroupsModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _userGroupsService = userGroupsService;
            _userGroupsModelFactory = userGroupsModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }
        
        public virtual ActionResult Grid()
        {
            ViewBag.DataSource = Url.Action("GridDataSource");
            return PartialView();
        }

        [UserHasAtLeastOnePermission("ManageUserGroups_Add", "ManageUserGroups_Edit")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _userGroupsService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _userGroupsModelFactory.PrepareUserGroupModelAsync(record));
                }
            }

            return View(await _userGroupsModelFactory.PrepareUserGroupModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageUserGroups_Add", "ManageUserGroups_Edit")]
        public virtual async Task<ActionResult> Editor(UserGroupModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _userGroupsModelFactory.PrepareTblUserGroup(model);
            var recordId = model.Id;
            try
            {

                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageUserGroups_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    recordId = await _userGroupsService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageUserGroups_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _userGroupsService.UpdateAsync(record);
                }

                await _localizedEntityService.SaveAllLocalizedStringsAsync(record, model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManageUserGroups", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshUserGroupsGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageUserGroups_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _userGroupsService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var query =  _userGroupsService.GetAsQueryable();
            var strUnlimited = _localizationService.GetResource("Unlimited");
            var strPer = $" {_localizationService.GetResource("Per")} ";
            var dataSource = query.Select(p => new
            {
                p.Id,
                p.GroupName,
                p.GroupDisplayOrder,
                p.GroupPriority,
                DownloadLimit = p.MaxDownloadCount > 0
                    ? p.MaxDownloadCount + strPer + p.MaxDownloadPeriodType
                    : strUnlimited,
                SubscriptionExpirationTime = p.SubscriptionExpirationTime > 0
                    ? p.SubscriptionExpirationTime + " " + p.SubscriptionExpirationPeriodType
                    : strUnlimited,
                p.Published,
                p.SubscriptionFee,
                p.SubscriptionDiscountPercentage,
                p.DiscountForRenewalBeforeExpiration,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}