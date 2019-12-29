using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Products;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageProductDiscountsForUserGroups")]
    public partial class ProductDiscountsForUserGroupsController : BaseController
    {
        private readonly IProductDiscountsForUserGroupsModelFactory _modelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IProductDiscountsForUserGroupsService _productDiscountsForUserGroupsService;

        public ProductDiscountsForUserGroupsController(
            IProductDiscountsForUserGroupsModelFactory modelFactory,
            ILocalizationService localizationService,
            IProductDiscountsForUserGroupsService productDiscountsForUserGroupsService)
        {
            _modelFactory = modelFactory;
            _localizationService = localizationService;
            _productDiscountsForUserGroupsService = productDiscountsForUserGroupsService;
        }


        public virtual ActionResult Grid(int productId)
        {
            ViewBag.DataSource = Url.Action("GridDataSource", new { productId = productId });
            ViewBag.ProductId = productId;
            return PartialView();
        }

        [UserHasAtLeastOnePermission("ManageProductDiscountsForUserGroups_Add", "ManageProductDiscountsForUserGroups_Edit")]
        public virtual async Task<ActionResult> Editor(int? id, int productId)
        {
            if (id != null)
            {
                var record = await _productDiscountsForUserGroupsService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(_modelFactory.PrepareProductDiscountsForUserGroupsModel(record, productId));
                }
            }

            return View(_modelFactory.PrepareProductDiscountsForUserGroupsModel(null, productId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageProductDiscountsForUserGroups_Add", "ManageProductDiscountsForUserGroups_Edit")]
        public virtual async Task<ActionResult> Editor(ProductDiscountsForUserGroupsModel model, bool? saveAndContinue)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _modelFactory.PrepareTblProductDiscountsForUserGroups(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageProductDiscountsForUserGroups_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await _productDiscountsForUserGroupsService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageProductDiscountsForUserGroups_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _productDiscountsForUserGroupsService.UpdateAsync(record);
                }
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            if (saveAndContinue != null && saveAndContinue.Value)
            {
                return RedirectToAction("Editor", "ProductDiscountsForUserGroups", new { productId=record.ProductId, id = recordId });
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshProductDiscountsForUserGroupsGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageProductDiscountsForUserGroups_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _productDiscountsForUserGroupsService.DeleteAsync(key);
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
            var query = _productDiscountsForUserGroupsService.GetAsQueryable(productId);

            var dataSource = query.Select(p => new
            {
                p.Id,
                p.ApplyDiscountToProductAttributes,
                p.ApplyDiscountToHigherUserGroups,
                p.DiscountPercent,
                UserGroup = p.UserGroup.GroupName
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}