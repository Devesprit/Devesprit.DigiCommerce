using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;
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
    public partial class PostAttributesMappingController : BaseController
    {
        private readonly IPostAttributeMappingModelFactory _postAttributeMappingModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPostAttributesService _postAttributesService;
        private readonly IPostAttributesMappingService _postAttributesMappingService;

        public PostAttributesMappingController(IPostAttributeMappingModelFactory postAttributeMappingModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IPostAttributesService postAttributesService,
            IPostAttributesMappingService postAttributesMappingService)
        {
            _postAttributeMappingModelFactory = postAttributeMappingModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _postAttributesService = postAttributesService;
            _postAttributesMappingService = postAttributesMappingService;
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
                var record = await _postAttributesMappingService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _postAttributeMappingModelFactory.PreparePostAttributeMappingModelAsync(record, postId));
                }
            }

            return View(await _postAttributeMappingModelFactory.PreparePostAttributeMappingModelAsync(null, postId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(PostAttributeMappingModel model, bool? saveAndContinue)
        {
            if (model.AttributeOptionId == null && string.IsNullOrWhiteSpace(model.Value[0]))
            {
                ModelState.AddModelError("AttributeOptionId", string.Format(_localizationService.GetResource("FieldRequired"), _localizationService.GetResource("Option")));
                ModelState.AddModelError("Value", string.Format(_localizationService.GetResource("FieldRequired"), _localizationService.GetResource("Value")));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _postAttributeMappingModelFactory.PrepareTblPostAttributesMapping(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _postAttributesMappingService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _postAttributesMappingService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "PostAttributesMapping", new { postId = record.PostId, id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshPostAttributesGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _postAttributesMappingService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        public virtual async Task<ActionResult> GetAttributeOptions(int attributeId)
        {
            var attribute = await _postAttributesService.FindByIdAsync(attributeId);
            return Json(
                new
                {
                    options = attribute.Options.Select(p => new { text = p.Name, id = p.Id }).ToArray(),
                    typeIsOption = attribute.AttributeType == PostAttributeType.Option
                },
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GridDataSource(DataManager dm, int postId)
        {
            var query = _postAttributesMappingService.GetAsQueryable(postId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                AttributeName = p.PostAttribute.Name,
                AttributeType = p.PostAttribute.AttributeType + "",
                AttributeValue = p.AttributeOptionId != null ? p.AttributeOption.Name : p.Value,
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}