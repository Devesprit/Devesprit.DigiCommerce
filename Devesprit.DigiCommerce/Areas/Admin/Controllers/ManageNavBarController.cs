using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.NavBar;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageNavBar")]
    public partial class ManageNavBarController : BaseController
    {
        private readonly INavBarService _navBarService;
        private readonly INavBarItemModelFactory _navBarItemModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManageNavBarController(INavBarService navBarService,
            INavBarItemModelFactory navBarItemModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _navBarService = navBarService;
            _navBarItemModelFactory = navBarItemModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var record = await _navBarService.FindByIdAsync(id);
            ViewBag.EditorTitle = _localizationService.GetResource("Edit") + " (" + _localizationService.GetResource("Name") + ": " + record.Name + ")";
            return PartialView("Editor", await _navBarItemModelFactory.PrepareNavBarItemModelAsync(record));
        }

        public virtual async Task<ActionResult> AddNew(int? parentId)
        {
            var model = await _navBarItemModelFactory.PrepareNavBarItemModelAsync(null);
            model.ParentItemId = parentId;
            if (parentId == null)
            {
                ViewBag.EditorTitle = _localizationService.GetResource("Add") + " (" + _localizationService.GetResource("SubsetOf") + ": " + _localizationService.GetResource("Root") + ")";
            }
            else
            {
                var parentNode = await _navBarService.FindByIdAsync(parentId.Value);
                ViewBag.EditorTitle = _localizationService.GetResource("Add") + " (" + _localizationService.GetResource("SubsetOf") + ": " + parentNode.Name + ")";
            }
            return PartialView("Editor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Save(NavBarItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("Editor", model);
            }

            var record = _navBarItemModelFactory.PrepareTblNavBarItems(model);

            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageNavBar_Add"))
                    {
                        ModelState.AddModelError("", _localizationService.GetResource("AccessPermissionErrorDesc"));
                        return PartialView("Editor", model);
                    }

                    //Add new record
                    await _navBarService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageNavBar_Edit"))
                    {
                        ModelState.AddModelError("", _localizationService.GetResource("AccessPermissionErrorDesc"));
                        return PartialView("Editor", model);
                    }

                    //Edit record
                    await _navBarService.UpdateAsync(record);
                }

                await _localizedEntityService.SaveAllLocalizedStringsAsync(record, model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return PartialView("Editor", model);
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                refreshNavBarTreeView();
                                DestroyEditor();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageNavBar_Delete")]
        public virtual async Task<ActionResult> Delete(int keys)
        {
            try
            {
                await _navBarService.DeleteAsync(keys);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        [UserHasPermission("ManageNavBar_ChangeOrder")]
        public virtual async Task<ActionResult> ChangeIndex(int[] nodesOrder, int id, int? newParentId)
        {
            try
            {
                await _navBarService.SetNavbarItemsIndexAsync(nodesOrder, id, newParentId);
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
            var dataSource = _navBarService.GetAsQueryable();

            var result = dataSource.ApplyDataManager(dm, out int count).ToList().OrderBy(p=> p.Index).Select(p => new
            {
                p.Id,
                p.ParentItemId,
                p.Name,
            });
            return Json(result);
        }
    }
}
