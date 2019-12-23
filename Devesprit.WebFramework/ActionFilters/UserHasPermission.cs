using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.WebFramework.Helpers;

namespace Devesprit.WebFramework.ActionFilters
{
    public partial class UserHasPermission: ActionFilterAttribute
    {
        public string AreaName { get; }
        public string Controller { get; set; }
        public string Action { get; set; }

        public UserHasPermission(string areaName)
        {
            AreaName = areaName;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.HasPermission(AreaName))
            {
                if (string.IsNullOrWhiteSpace(Controller) || string.IsNullOrWhiteSpace(Action))
                {
                    Controller = "Error";
                    Action = "AccessPermissionError";
                }
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                        {
                            controller = Controller,
                            action = Action,
                            Area = string.Empty
                    }
                    ));
            }

            base.OnActionExecuting(filterContext);
        }
    }
}