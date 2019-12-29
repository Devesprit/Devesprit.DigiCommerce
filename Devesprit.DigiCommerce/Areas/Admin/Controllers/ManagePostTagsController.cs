using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Posts;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManagePostTags")]
    public partial class ManagePostTagsController : BaseController
    {
        private readonly IPostTagsService _postTagsService;
        private readonly IPostTagsModelFactory _postTagsModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public ManagePostTagsController(IPostTagsService postTagsService,
            IPostTagsModelFactory postTagsModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _postTagsService = postTagsService;
            _postTagsModelFactory = postTagsModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [UserHasAtLeastOnePermission("ManagePostTags_Add", "ManagePostTags_Edit")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _postTagsService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postTagsModelFactory.PreparePostTagModelAsync(record));
                }
            }

            return View(await _postTagsModelFactory.PreparePostTagModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManagePostTags_Add", "ManagePostTags_Edit")]
        public virtual async Task<ActionResult> Editor(PostTagModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _postTagsModelFactory.PrepareTblPostTags(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManagePostTags_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await _postTagsService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManagePostTags_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _postTagsService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ManagePostTags", new { id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostTagsGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManagePostTags_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postTagsService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpGet]
        public virtual async Task<ActionResult> PostTagsSuggestion(string query)
        {
            var tags = await _postTagsService.TagSuggestionAsync(query);
            return Json(new {results = tags.Select(p => new {text = p.Tag, id = p.Tag }).ToArray()},
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var query = _postTagsService.GetAsQueryable();

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.Tag
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}