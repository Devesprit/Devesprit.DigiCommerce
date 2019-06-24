using System.Web.Routing;

namespace Devesprit.WebFramework.Routes
{
    public partial interface IRoutePublisher
    {
        void RegisterRoutes(RouteCollection routes);
    }
}
