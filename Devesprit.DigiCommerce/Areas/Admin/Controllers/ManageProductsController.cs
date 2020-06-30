using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.FileServers;
using Devesprit.Services.Products;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework.ActionFilters;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [UserHasPermission("ManageProducts")]
    public partial class ManageProductsController : BaseController
    {
        private readonly IAdminPanelProductService _adminPanelProductService;
        private readonly IAdminProductModelFactory _adminProductModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IFileServersService _fileServersService;

        public ManageProductsController(IAdminPanelProductService adminPanelProductService, 
            IAdminProductModelFactory adminProductModelFactory,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IFileServersService fileServersService)
        {
            _adminPanelProductService = adminPanelProductService;
            _adminProductModelFactory = adminProductModelFactory;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _fileServersService = fileServersService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        
        public virtual ActionResult Grid(int? categoryId)
        {
            if (categoryId != null && categoryId > 0)
            {
                //Filter by category
                ViewBag.DataSource = Url.Action("GridDataSource", new { categoryId = categoryId });
            } 
            else
            {
                ViewBag.DataSource = Url.Action("GridDataSource");
            }
            
            return PartialView();
        } 
         
        [UserHasAtLeastOnePermission("ManageProducts_Add", "ManageProducts_Edit")]
        public virtual async Task<ActionResult> Editor(int? id)
        {
            if (id != null)
            {
                var record = await _adminPanelProductService.FindByIdAsync(id.Value);
                if (record != null)
                {
                    return View(await _adminProductModelFactory.PrepareProductModelAsync(record));
                }
            }

            return View(await _adminProductModelFactory.PrepareProductModelAsync(null));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserHasAtLeastOnePermission("ManageProducts_Add", "ManageProducts_Edit")]
        public virtual async Task<ActionResult> Editor(ProductModel model, bool? saveAndContinue)
        {
            if (!model.Slug.IsNormalizedUrl())
            {
                ModelState.AddModelError("Slug", string.Format(_localizationService.GetResource("InvalidFieldData"), _localizationService.GetResource("Slug")));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var record = _adminProductModelFactory.PrepareTblProducts(model);
            var recordId = model.Id;
            try
            {
                if (model.Id == null)
                {
                    if (!HttpContext.UserHasPermission("ManageProducts_Add"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Add new record
                    recordId = await _adminPanelProductService.AddAsync(record);
                }
                else
                {
                    if (!HttpContext.UserHasPermission("ManageProducts_Edit"))
                    {
                        return View("AccessPermissionError");
                    }

                    //Edit record
                    await _adminPanelProductService.UpdateAsync(record);
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
                return RedirectToAction("Editor", "ManageProducts", new {id = recordId});
            }

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                                window.opener.refreshProductsGrid();
                             </script>");
        }

        [HttpPost]
        [UserHasPermission("ManageProducts_Delete")]
        public virtual async Task<ActionResult> Delete(int[] keys)
        {
            try
            {
                foreach (var key in keys)
                    await _adminPanelProductService.DeleteAsync(key);

                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        [HttpPost]
        [UserHasPermission("ManageProducts_Delete")]
        public virtual async Task<ActionResult> DeleteProductWithFiles(int id, bool? deleteFiles)
        {
            try
            {
                if (deleteFiles != null && deleteFiles.Value == true)
                {
                    var product = await _adminPanelProductService.FindByIdAsync(id);
                    var fileServer = _fileServersService.GetWebService(product.FileServer);
                    if (!string.IsNullOrWhiteSpace(product.FilesPath))
                    {
                        await fileServer.DeleteDirectoryAsync(product.FilesPath);
                    }
                    if (!string.IsNullOrWhiteSpace(product.DemoFilesPath))
                    {
                        await fileServer.DeleteDirectoryAsync(product.DemoFilesPath);
                    }
                }
                await _adminPanelProductService.DeleteAsync(id);

                return Content("OK");
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return Content(string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
            }
        }

        public virtual ActionResult GridDataSource(DataManager dm, int? categoryId)
        {
            var query = _adminPanelProductService.GetAsQueryable();
            if (categoryId != null && categoryId > 0)
            {
                query = query.Where(p => p.Categories.Any(x => x.Id == categoryId));
            }
            var postUrl = Url.Action("Index", "Product", new { area = "" });
            var dataSource = query.Select(p => new
            {
                p.Id,
                NumberOfDownloads = p.DownloadsLog.Count,
                FileServer = p.FileServer.FileServerName ?? "-",
                DownloadLimitedToUserGroup = p.DownloadLimitedToUserGroup.GroupName ?? "-",
                p.IsFeatured,
                LastUpDate = p.LastUpDate ?? p.PublishDate,
                p.PinToTop,
                p.Price,
                p.Title,
                p.PublishDate,
                PurchaseExpiration = p.PurchaseExpiration > 0
                    ? p.PurchaseExpiration + " " + p.PurchaseExpirationTimeType
                    : "-",
                p.ShowInHotList,
                p.NumberOfViews,
                p.Published,
                p.RenewalPrice,
                p.AllowCustomerReviews,
                p.PageTitle,
                p.Slug,
                ProductUrl = "<a target='_blank' href='" + postUrl + "/" + p.Id + "/" + p.Slug + "'>" + p.Title + "</a>"
            });

            var result = dataSource.ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }
    }
}