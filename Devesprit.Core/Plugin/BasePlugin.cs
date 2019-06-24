using System.Web.Routing;

namespace Devesprit.Core.Plugin
{
    public abstract partial class BasePlugin : IPlugin
    {
        public virtual PluginDescriptor PluginDescriptor { get; set; }

        public abstract void GetConfigurationRoute(out string actionName, out string controllerName,
            out RouteValueDictionary routeValues);

        public virtual void Install()
        {
            PluginManager.MarkPluginAsInstalled(this.PluginDescriptor.SystemName);
        }

        public virtual void Uninstall()
        {
            PluginManager.MarkPluginAsUninstalled(this.PluginDescriptor.SystemName);
        }

    }
}
