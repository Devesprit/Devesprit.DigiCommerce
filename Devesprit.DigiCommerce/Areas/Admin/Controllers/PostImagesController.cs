using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Posts;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class PostImagesController : BaseController
    {
        private readonly IPostImageModelFactory _postImageModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPostImagesService _postImagesService;

        public PostImagesController(
            IPostImageModelFactory postImageModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IPostImagesService postImagesService)
        {
            _postImageModelFactory = postImageModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _postImagesService = postImagesService;
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
                var record = await _postImagesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postImageModelFactory.PreparePostImageModelAsync(record, postId));
                }
            }

            return View(await _postImageModelFactory.PreparePostImageModelAsync(null, postId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(PostImageModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = await _postImageModelFactory.PrepareTblPostImagesAsync(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _postImagesService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _postImagesService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "PostImages", new { postId=record.PostId , id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostImagesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postImagesService.DeleteAsync(key);
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
            var query = _postImagesService.GetAsQueryable(postId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.Title,
                p.Alt,
                p.ImageUrl
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}