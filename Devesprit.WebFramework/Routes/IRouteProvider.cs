using System.Web.Routing;

namespace Devesprit.WebFramework.Routes
{
    public partial interface IRouteProvider
    {
        void RegisterRoutes(RouteCollection routes);

        int Priority { get; }
    }
}
