using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Services.Localization;
using Devesprit.Services.Pages;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class HomeController : BaseController
    {
        private readonly IPagesService _pagesService;

        public HomeController(IPagesService pagesService)
        {
            _pagesService = pagesService;
        }

        public virtual async Task<ActionResult> Index()
        {
            var defaultPage = await _pagesService.GetWebsiteDefaultPageAsync();
            if (defaultPage != null)
            {
                //Current page editor page URL (for Admin User)
                ViewBag.AdminEditCurrentPage =
                    $"PopupWindows('{Url.Action("Editor", "ManagePages", new {area = "Admin"})}', 'PageEditor', 1200, 670, {{ id: {defaultPage.Id} }}, 'get')";

                return View("../Page/Index", defaultPage);
            }

            return View();
        }

        public virtual ActionResult LoadingPage()
        {
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
    }
}