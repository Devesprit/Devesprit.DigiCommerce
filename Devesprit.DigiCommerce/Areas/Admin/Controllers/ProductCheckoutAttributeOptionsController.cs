using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Products;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ProductCheckoutAttributeOptionsController : BaseController
    {
        private readonly IProductCheckoutAttributeModelFactory _productCheckoutAttributeModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;

        public ProductCheckoutAttributeOptionsController(IProductCheckoutAttributeModelFactory productCheckoutAttributeModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IProductCheckoutAttributesService productCheckoutAttributesService)
        {
            _productCheckoutAttributeModelFactory = productCheckoutAttributeModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
        }


        [ChildActionOnly]
        public virtual ActionResult Grid(int attributeId)
        {
            ViewBag.DataSource = Url.Action("GridDataSource", new { attributeId = attributeId });
            ViewBag.AttributeId = attributeId;
            return PartialView();
        }

        public virtual async Task<ActionResult> Editor(int? id, int attributeId)
        {
            if (id != null)
            {
                var record = await _productCheckoutAttributesService.FindOptionByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _productCheckoutAttributeModelFactory.PrepareProductCheckoutAttributeOptionModelAsync(record, attributeId));
                }
            }

            return View(await _productCheckoutAttributeModelFactory.PrepareProductCheckoutAttributeOptionModelAsync(null, attributeId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(ProductCheckoutAttributeOptionModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _productCheckoutAttributeModelFactory.PrepareTblProductCheckoutAttributeOptions(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _productCheckoutAttributesService.AddOptionAsync(record);
                }
                else
                {
                    //Edit record
                    await _productCheckoutAttributesService.UpdateOptionAsync(record);
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
                return RedirectToAction("Editor", "ProductCheckoutAttributeOptions",
                    new {attributeId = record.ProductCheckoutAttributeId, id = recordId});
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshProductCheckoutAttributeOptionsGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _productCheckoutAttributesService.DeleteOptionAsync(key);
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
            var query = _productCheckoutAttributesService.GetOptionsAsQueryable(attributeId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.Name,
                p.FilesPath,
                p.Price,
                p.RenewalPrice,
                PurchaseExpiration = p.PurchaseExpiration > 0 ?
                    p.PurchaseExpiration + " " + p.PurchaseExpirationTimeType
                    : "-",
                FileServer = p.FileServer.FileServerName ?? "-",
                DownloadLimitedToUserGroup = p.DownloadLimitedToUserGroup.GroupName ?? "-",
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}