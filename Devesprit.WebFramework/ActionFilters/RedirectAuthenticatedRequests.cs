using System.Web.Mvc;
using System.Web.Routing;

namespace Devesprit.WebFramework.ActionFilters
{
    public partial class RedirectAuthenticatedRequests : ActionFilterAttribute
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(Controller) || string.IsNullOrWhiteSpace(Action))
                {
                    Controller = "Home";
                    Action = "Index";
                }
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                        {
                            controller = Controller,
                            action = Action
                        }
                    ));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}