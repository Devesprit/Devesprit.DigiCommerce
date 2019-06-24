using System.Web.Routing;

namespace Devesprit.Core.Plugin
{
    public partial interface IPlugin
    {
        PluginDescriptor PluginDescriptor { get; set; }

        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

        void Install();

        void Uninstall();
    }
}
