using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Services.Pages;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class PageController : BaseController
    {
        private readonly IPagesService _pagesService;

        public PageController(IPagesService pagesService)
        {
            _pagesService = pagesService;
        }

        // GET: Page
        [Route("{lang}/Pages/{slug}", Order = 0)]
        [Route("Pages/{slug}", Order = 1)]
        public virtual async Task<ActionResult> Index(string slug)
        {
            var page = await _pagesService.FindBySlugAsync(slug);
            if (page == null || !page.Published)
            {
                return View("PageNotFound");
            }

            //Current page editor page URL (for Admin User)
            ViewBag.AdminEditCurrentPage =
                $"PopupWindows('{Url.Action("Editor", "ManagePages", new { area = "Admin" })}', 'PageEditor', 1200, 670, {{ id: {page.Id} }}, 'get')";

            return View("Index", page);
        }
    }
}