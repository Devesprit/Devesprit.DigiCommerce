using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.WebFramework.Routes;

namespace Devesprit.DigiCommerce
{
    public partial class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.MapMvcAttributeRoutes();

            var routePublisher = DependencyResolver.Current.GetService<IRoutePublisher>();
            routePublisher.RegisterRoutes(routes);

            // Localization route - it will be used as a route of the first priority 
            routes.MapRoute(
                name: "DefaultLocalized",
                url: "{lang}/{controller}/{action}/{id}",
                constraints: new { lang = @"(\w{2})|(\w{2}-\w{2})" },   // en or en-US
                defaults: new
                {
                    controller = "Home",
                    action = "Index",
                    id = UrlParameter.Optional,
                });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
