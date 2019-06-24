using System.Web.Mvc;

namespace Devesprit.DigiCommerce.Controllers
{
    public partial class KeepAliveController : Controller
    {
        public virtual ActionResult Index()
        {
            Response.AddHeader("Cache-Control", "no-cache");
            Response.AddHeader("Pragma", "no-cache");
            Response.ContentType = "text/plain";
            return Content("I am alive!");
        }
    }
}