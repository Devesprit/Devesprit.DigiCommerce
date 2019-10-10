using System.Web.Mvc;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Services.MemoryCache;

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