using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Pages;
using Z.EntityFramework.Plus;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class HomeController : BaseController
    {
        private readonly IPagesService _pagesService;
        private readonly AppDbContext _dbContext;

        public HomeController(IPagesService pagesService, AppDbContext dbContext)
        {
            _pagesService = pagesService;
            _dbContext = dbContext;
        }

        [MethodCache(Tags = new[] { nameof(TblBlogPosts), nameof(TblProducts) }, VaryByCustom = "lang" /*"lang,user"*/, DurationSec = 3600)]
        public virtual async Task<ActionResult> Index()
        {
            var defaultPage = await _pagesService.GetWebsiteDefaultPageAsync();
            if (defaultPage != null)
            {
                //Current page editor page URL (for Admin User)
                ViewBag.AdminEditCurrentPage =
                    $"PopupWindows('{Url.Action("Editor", "ManagePages", new { area = "Admin" })}', 'PageEditor', 1200, 670, {{ id: {defaultPage.Id} }}, 'get')";

                return View("../Page/Index", defaultPage);
            }

            return View();
        }

        public virtual ActionResult TermsAndConditions()
        {
            var terms = CurrentSettings.GetLocalized(p => p.TermsAndConditions);
            if (string.IsNullOrWhiteSpace(terms))
            {
                return View("PageNotFound", "Error");
            }

            ViewBag.TermsHtml = terms;
            return View();
        }

        public async Task<ActionResult> UpDateDB()
        {
            try
            {
                //Downloads Count
                var dc = await _dbContext.Database.ExecuteSqlCommandAsync(
                    "update Tbl_Products set [NumberOfDownloads] = (select COUNT(id) from [dbo].[Tbl_ProductDownloadsLog] where [ProductId] = Tbl_Products.Id)");

                //Likes Count
                var lc = await _dbContext.Database.ExecuteSqlCommandAsync(
                    "update Tbl_Posts set [NumberOfLikes] = (select COUNT(id) from [dbo].[Tbl_UserLikes] where [PostId] = Tbl_Posts.Id)");

                //Purchase Count
                var pc = await _dbContext.Database.ExecuteSqlCommandAsync(
                    "update Tbl_Products set [NumberOfPurchases] = (select COUNT(id) from [dbo].[Tbl_InvoiceDetails] where [ItemType] = 1 And [ItemId] = Tbl_Products.Id)");

                return Content("OK - "+dc+", "+lc+", "+pc);
            }
            catch (Exception e)
            {
                return Content(e.ToString());
            }
        }
    }
}