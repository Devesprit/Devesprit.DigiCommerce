using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Localization;
using Devesprit.Services.NavBar;
using Devesprit.Services.Posts;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManagePostCategoriesController : BaseController
    {
        private readonly IPostCategoriesService _postCategoriesService;
        private readonly IPostCategoriesModelFactory _postCategoriesModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INavBarService _navBarService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManagePostCategoriesController(IPostCategoriesService postCategoriesService,
            IPostCategoriesModelFactory postCategoriesModelFactory,
            ILocalizationService localizationService,
            INavBarService navBarService,
            ILocalizedEntityService localizedEntityService)
        {
            _postCategoriesService = postCategoriesService;
            _postCategoriesModelFactory = postCategoriesModelFactory;
            _localizationService = localizationService;
            _navBarService = navBarService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> Edit(int id)
        {
            var record = await _postCategoriesService.FindByIdAsync(id);
            ViewBag.EditorTitle = _localizationService.GetResource("Edit") + " (" + _localizationService.GetResource("Name") + ": " + record.CategoryName + ")";
            return PartialView("Editor", await _postCategoriesModelFactory.PreparePostCategoryModelAsync(record));
        }

        public virtual async Task<ActionResult> AddNew(int? parentId)
        {
            var model = await _postCategoriesModelFactory.PreparePostCategoryModelAsync(null);
            model.ParentCategoryId = parentId;
            if (parentId == null)
            {
                ViewBag.EditorTitle = _localizationService.GetResource("Add") + " (" + _localizationService.GetResource("SubsetOf") + ": " + _localizationService.GetResource("Root") + ")";
            }
            else
            {
                var parentNode = await _postCategoriesService.FindByIdAsync(parentId.Value);
                ViewBag.EditorTitle = _localizationService.GetResource("Add") + " (" + _localizationService.GetResource("SubsetOf") + ": " + parentNode.CategoryName + ")";
            }
            return PartialView("Editor", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Save(PostCategoryModel model)
        {
            if (!model.Slug.IsNormalizedUrl())
            {
                ModelState.AddModelError("Slug", string.Format(_localizationService.GetResource("InvalidFieldData"), _localizationService.GetResource("Slug")));
            }

            if (!ModelState.IsValid)
            {
                return PartialView("Editor", model);
            }

            var record = _postCategoriesModelFactory.PrepareTblPostCategories(model);

            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    await _postCategoriesService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _postCategoriesService.UpdateAsync(record);
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
                                refreshPostCategoriesTreeView();
                                DestroyEditor();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int keys)
        {
            try
            {  
                await _postCategoriesService.DeleteAsync(keys);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> ChangeIndex(int[] nodesOrder, int id, int? newParentId)
        {
            try
            {
                await _postCategoriesService.SetCategoryOrderAsync(nodesOrder, id, newParentId);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult TreeViewDataSource(DataManager dm)
        {
            var dataSource = _postCategoriesService.GetAsQueryable();

            var result = dataSource.ApplyDataManager(dm, out int count).ToList().OrderBy(p => p.DisplayOrder).Select(p => new
            {
                p.Id,
                p.ParentCategoryId,
                p.CategoryName, 
                p.Slug
            });
            return Json(result);
        }

        public virtual async Task<ActionResult> GenerateNavbar()
        {
            var idMap = new Dictionary<int, int>();
            foreach (var category in _postCategoriesService.GetAsEnumerable())
            {
                var id = await _navBarService.AddAsync(new TblNavBarItems()
                {
                    Index = category.DisplayOrder,
                    InnerHtml = category.CategoryName,
                    Name = category.CategoryName,
                    Url = "/Categories/" + category.Slug
                });

                var catLocals = await _localizedEntityService.GetLocalizedPropertiesAsync(category.Id, "TblPostCategories", "CategoryName");
                foreach (var local in catLocals)
                {
                    await _localizedEntityService.AddAsync(new TblLocalizedProperty()
                    {
                        LanguageId = local.LanguageId,
                        EntityId = id,
                        LocaleKeyGroup = "TblNavBarItems",
                        LocaleKey = "InnerHtml",
                        LocaleValue = local.LocaleValue
                    });
                }

                idMap.Add(category.Id, id);
            }

            foreach (var category in _postCategoriesService.GetAsEnumerable().Where(p=> p.ParentCategoryId != null))
            {
                var navbarItem = await _navBarService.FindByIdAsync(idMap[category.Id]);
                navbarItem.ParentItemId = idMap[category.ParentCategoryId.Value];
                await _navBarService.UpdateAsync(navbarItem);
            }

            SuccessNotification(_localizationService.GetResource("OperationCompletedSuccessfully"));

            return RedirectToAction("Index");
        }
    }
}