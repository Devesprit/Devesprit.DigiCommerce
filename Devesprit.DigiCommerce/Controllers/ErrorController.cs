using System.Globalization;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Controllers.Event;
using Devesprit.Services.MemoryCache;
using JetBrains.Annotations;

namespace Devesprit.DigiCommerce.Controllers
{
    [Intercept(typeof(MethodCache))]
    public partial class ErrorController : BaseController
    { 
        protected override void Execute(RequestContext requestContext)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(WorkContext.CurrentLanguage.IsoCode);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(WorkContext.CurrentLanguage.IsoCode);
              
            base.Execute(requestContext);
        }

        [MethodCache(VaryByCustom = "lang")]
        public virtual ActionResult PageNotFound([CanBeNull] string errorCode)
        {
            if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {  
                ViewBag.ErrorCode = errorCode;
            }

            EventPublisher.Publish(new PageNotFoundEvent(HttpContext, errorCode));

            return View("PageNotFound");
        }

        [MethodCache(VaryByCustom = "lang")]
        public virtual ActionResult Index([CanBeNull] string errorCode)
        {
            if (User != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
            {
                ViewBag.ErrorCode = errorCode;
            }

            EventPublisher.Publish(new ErrorEvent(HttpContext, errorCode));

            return View("Error");
        }

        [MethodCache(VaryByCustom = "lang")]
        public virtual ActionResult AccessPermissionError()
        {
            return View("AccessPermissionError");
        }
    }
}