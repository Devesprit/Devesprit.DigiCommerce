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
    [UserHasPermission("ManagePostAttributes_Options")]
    public partial class PostAttributeOptionsController : BaseController
    {
        private readonly IPostAttributesService _postAttributesService;
        private readonly IPostAttributesModelFactory _postAttributesModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;

        public PostAttributeOptionsController(IPostAttributesService postAttributesService,
            IPostAttributesModelFactory postAttributesModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService)
        {
            _postAttributesService = postAttributesService;
            _postAttributesModelFactory = postAttributesModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
        }


        [ChildActionOnly]
        public virtual ActionResult Grid(int attributeId)
        {
            ViewBag.DataSource = Url.Action("GridDataSource", new { attributeId = attributeId });
            ViewBag.AttributeId = attributeId;
            return PartialView();
        }

        [UserHasAtLeastOnePermission("ManagePostAttributes_Options_Add", "ManagePostAttributes_Options_Edit")]
        public virtual async Task<ActionResult> Editor(int attributeId, int? id)
        {
            if (id != null)
            {
                var record = await _postAttributesService.FindOptionByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postAttributesModelFactory.PreparePostAttributeOptionModelAsync(record, attributeId));
                }
            }

            return View(
                await _postAttributesModelFactory.PreparePostAttributeOptionModelAsync(null, attributeId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManagePostAttributes_Options_Add", "ManagePostAttributes_Options_Edit")]
        public virtual async Task<ActionResult> Editor(PostAttributeOptionModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _postAttributesModelFactory.PrepareTblPostAttributeOptions(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManagePostAttributes_Options_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await _postAttributesService.AddOptionAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManagePostAttributes_Options_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _postAttributesService.UpdateOptionAsync(record);
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
                return RedirectToAction("Editor", "PostAttributeOptions",
                    new {attributeId = record.PostAttributeId, id = recordId});
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostAttributeOptionsGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManagePostAttributes_Options_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postAttributesService.DeleteOptionAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int attributeId)
        {
            var query = _postAttributesService.GetOptionsAsQueryable(attributeId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.Name
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}