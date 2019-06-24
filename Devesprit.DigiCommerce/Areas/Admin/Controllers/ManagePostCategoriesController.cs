using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
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
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManagePostCategoriesController(IPostCategoriesService postCategoriesService,
            IPostCategoriesModelFactory postCategoriesModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _postCategoriesService = postCategoriesService;
            _postCategoriesModelFactory = postCategoriesModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _postCategoriesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postCategoriesModelFactory.PreparePostCategoryModelAsync(record));
                }
            }

            return View(await _postCategoriesModelFactory.PreparePostCategoryModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(PostCategoryModel model, bool? saveAndContinue)
        {
            if (!model.Slug.IsNormalizedUrl())
            {
                ModelState.AddModelError("Slug", string.Format(_localizationService.GetResource("InvalidFieldData"), _localizationService.GetResource("Slug")));
            }

            if (!ModelState.IsValid)
            {
                model.CategoriesList = await _postCategoriesService.GetAsSelectListAsync();
                return View(model);
            }

            var record = _postCategoriesModelFactory.PrepareTblPostCategories(model);
            var recordId = model.Id; 
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _postCategoriesService.AddAsync(record);
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
                model.CategoriesList = await _postCategoriesService.GetAsSelectListAsync();
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ManagePostCategories", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostCategoriesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postCategoriesService.DeleteAsync(key);
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
            var query = _postCategoriesService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.CategoryName,
                p.Slug,
                ParentCategoryName = p.ParentCategory.CategoryName ?? ""
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}