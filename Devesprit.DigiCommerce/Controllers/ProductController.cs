﻿using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Data.Enums;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.DigiCommerce.Models.Products;
using Devesprit.Services;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Microsoft.AspNet.Identity;
using X.PagedList;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class ProductController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IPostCategoriesService _categoriesService;

        public ProductController(
            IProductService productService,
            IProductModelFactory productModelFactory,
            IPostCategoriesService categoriesService)
        {
            _productService = productService;
            _productModelFactory = productModelFactory;
            _categoriesService = categoriesService;
        }

        // GET: Product
        [Route("{lang}/product/{id}/{slug}", Order = 0)]
        [Route("product/{id}/{slug}", Order = 1)]
        [Route("{lang}/product/{slug}", Order = 2)]
        [Route("product/{slug}", Order = 3)]
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang" /*"lang,user"*/)]
        public virtual async Task<ActionResult> Index(int? id, string slug) 
        {
            var currentUser = await UserManager.FindByIdAsync(HttpContext.User.Identity.GetUserId());
            var isAdmin = HttpContext.User.IsInRole("Admin");
            
            TblProducts product = null;
            if (id != null)
            {
                product = await _productService.FindByIdAsync(id.Value);
            }

            if (product == null)
            {
                product = await _productService.FindBySlugAsync(slug);
            }

            if (product == null && int.TryParse(slug, out int productId))
            {
                product = await _productService.FindByIdAsync(productId);
            }

            if (product == null || (!product.Published && !isAdmin))
            {
                return View("PageNotFound");
            }

            var pageMainUrl = "";
            if (CurrentSettings.AppendLanguageCodeToUrl)
            {
                pageMainUrl = Url.Action("Index", "Product",
                    new {id = product.Id, slug = product.Slug, lang = WorkContext.CurrentLanguage.IsoCode},
                    Request.Url.Scheme);
            }
            else
            {
                pageMainUrl = Url.Action("Index", "Product", new { id = product.Id, slug = product.Slug }, Request.Url.Scheme);
            }
            if (Request.Url.ToString().Trim().ToLower() != pageMainUrl.Trim().ToLower())
            {
                Response.Clear();
                return RedirectPermanent(pageMainUrl.Trim().TrimEnd('/'));
            }

            //Increase the number of product views
            await _productService.IncreaseNumberOfViewsAsync(product);

            //Current product editor page URL (for Admin User)
            ViewBag.AdminEditCurrentPage =
                $"PopupWindows('{Url.Action("Editor", "ManageProducts", new { area = "Admin" })}', 'ProductEditor', 1200, 700, {{ id: {product.Id} }}, 'get')";

            return View(_productModelFactory.PrepareProductModel(product, currentUser, Url));
        } 

        [Route("{lang}/products/{listType}/{page?}", Order = 0)]
        [Route("products/{listType}/{page?}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang" /*"lang,user"*/)]
        public virtual ActionResult ProductsExplorer(ProductsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate)
        {
            return View(new ProductsExplorerModel()
            {
                PageIndex = page ?? 1,
                PageSize = pageSize,
                PostsListType = listType,  
                FilterByCategoryId = catId,
                FromDate = fromDate
            });
        }

        [ChildActionOnly]
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang" /*"lang,user"*/)]
        public virtual ActionResult GetProductsList(ProductsListType listType, int? page, int? pageSize, int? catId, DateTime? fromDate, ViewStyles? style, string wrapperStart, string wrapperEnd, bool? showPager)
        {
            var currentUser = UserManager.FindById(User.Identity.GetUserId());
            IPagedList<TblProducts> products = null;
            switch (listType)
            { 
                case ProductsListType.Newest:
                    products = _productService.GetNewItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.MostPopular:
                    products = _productService.GetPopularItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.HotList:
                    products = _productService.GetHotList(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.Featured:
                    products = _productService.GetFeaturedItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.BestSelling:
                    products = _productService.GetBestSelling(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.MostDownloaded:
                    products = _productService.GetMostDownloadedItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
                case ProductsListType.FreeProducts:
                    products = _productService.GetFreeItems(page ?? 1, pageSize ?? 24, catId, fromDate);
                    break;
            }
            var model = new ProductsListModel()
            {   
                PostsList = _productModelFactory.PrepareProductCardViewModel(products, currentUser, Url),
                ViewStyle = style ?? ViewStyles.Normal,
                PageIndex = page ?? 1,
                PageSize = pageSize,
                PostsListType = listType,
                FromDate = fromDate,
                FilterByCategoryId = catId,
                ShowPager = showPager ?? true,
                ItemWrapperStart = string.IsNullOrWhiteSpace(wrapperStart) ? "<div class='col-12 col-sm-6 col-lg-4 col-xl-3 py-2'>" : wrapperStart,
                ItemWrapperEnd = string.IsNullOrWhiteSpace(wrapperEnd) ? "</div>" : wrapperEnd
            };
            
            return View("Partials/_ProductsList", model);
        }

        [Route("{lang}/categories/{slug}/{page?}", Order = 0)]
        [Route("categories/{slug}/{page?}", Order = 1)]
        [MethodCache(Tags = new[] { nameof(TblProducts) }, VaryByCustom = "lang" /*"lang,user"*/)]
        public virtual async Task<ActionResult> FilterByCategory(string slug, int? page, int? pageSize)
        {
            var category = await _categoriesService.FindBySlugAsync(slug);
            if (category == null && int.TryParse(slug, out int categoryId))
            {
                category = await _categoriesService.FindByIdAsync(categoryId);
            }

            if (category == null || (category.DisplayArea != DisplayArea.ProductsSection && category.DisplayArea != DisplayArea.Both))
            {
                return View("PageNotFound");
            }

            return View(new ProductsExplorerModel()
            {
                CategoryName = category.GetLocalized(p => p.CategoryName),
                FilterByCategoryId = category.Id,
                PageIndex = page ?? 1,
                PageSize = pageSize,
                CategorySlug = slug
            });
        }
    }
}