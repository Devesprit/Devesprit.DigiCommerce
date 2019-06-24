using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class PostDescriptionsController : BaseController
    {
        private readonly IPostDescriptionModelFactory _postDescriptionModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPostDescriptionsService _postDescriptionsService;

        public PostDescriptionsController(
            IPostDescriptionModelFactory postDescriptionModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IPostDescriptionsService postDescriptionsService)
        {
            _postDescriptionModelFactory = postDescriptionModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _postDescriptionsService = postDescriptionsService;
        }
        
        public virtual ActionResult Grid(int postId)
        {
            ViewBag.DataSource = Url.Action("GridDataSource", new { postId = postId });
            ViewBag.PostId = postId;
            return PartialView();
        }

        public virtual async Task<ActionResult> Editor(int? id, int postId)
        {
            if (id != null)
            {
                var record = await _postDescriptionsService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postDescriptionModelFactory.PreparePostDescriptionModelAsync(record, postId));
                }
            }

            return View(await _postDescriptionModelFactory.PreparePostDescriptionModelAsync(null, postId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(PostDescriptionModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _postDescriptionModelFactory.PrepareTblPostDescriptions(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _postDescriptionsService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _postDescriptionsService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "PostDescriptions", new { postId = record.PostId, id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostDescriptionsGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postDescriptionsService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int postId)
        {
            var query = _postDescriptionsService.GetAsQueryable(postId);
           
            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.Title,
                p.HtmlDescription,
                p.AddToSearchEngineIndexes
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList().Select(p=> new
            {
                p.Id,
                p.DisplayOrder,
                p.Title,
                TextDescription = p.HtmlDescription.StripHtml(),
                p.AddToSearchEngineIndexes
            });
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}