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
    public partial class ProductCheckoutAttributesController : BaseController
    {
        private readonly IProductCheckoutAttributeModelFactory _productCheckoutAttributeModelFactory;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductCheckoutAttributesService _productCheckoutAttributesService;

        public ProductCheckoutAttributesController(IProductCheckoutAttributeModelFactory productCheckoutAttributeModelFactory,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IProductCheckoutAttributesService productCheckoutAttributesService)
        {
            _productCheckoutAttributeModelFactory = productCheckoutAttributeModelFactory;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _productCheckoutAttributesService = productCheckoutAttributesService;
        }

        public virtual ActionResult Grid(int productId)
        {
            ViewBag.DataSource = Url.Action("GridDataSource", new { productId = productId });
            ViewBag.ProductId = productId;
            return PartialView();
        }

        public virtual async Task<ActionResult> Editor(int? id, int productId)
        {
            if (id != null)
            {
                var record = await _productCheckoutAttributesService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _productCheckoutAttributeModelFactory.PrepareProductCheckoutAttributeModelAsync(record, productId));
                }
            }

            return View(await _productCheckoutAttributeModelFactory.PrepareProductCheckoutAttributeModelAsync(null, productId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Editor(ProductCheckoutAttributeModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _productCheckoutAttributeModelFactory.PrepareTblProductCheckoutAttributes(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    //Add new record
                    recordId = await _productCheckoutAttributesService.AddAsync(record);
                }
                else
                {
                    //Edit record
                    await _productCheckoutAttributesService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ProductCheckoutAttributes", new { productId=record.ProductId, id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshProductCheckoutAttributeGrid();
                             </script>");
        }

        [HttpPost]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _productCheckoutAttributesService.DeleteAsync(key);
                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int productId)
        {
            var query = _productCheckoutAttributesService.GetAsQueryable(productId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.DisplayOrder,
                p.Name,
                p.Required,
                AttributeType = p.AttributeType + ""
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}