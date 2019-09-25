using System.Web.Mvc;
using System.Web.UI;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class KeepAliveController : Controller
    {
        [OutputCache(Duration = 60 * 60, Location = OutputCacheLocation.Server, VaryByParam = "none")]
        public virtual ActionResult Index()
        {
            Response.AddHeader("Cache-Control", "no-cache");
            Response.AddHeader("Pragma", "no-cache");
            Response.ContentType = "text/plain";
            return Content("I am alive!");
        }
    }
}