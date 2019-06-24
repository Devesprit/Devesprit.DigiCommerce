using System.Collections.Generic;
using System.Web.Routing;
using Devesprit.Core.Plugin;

namespace Devesprit.Services.Widget
{
    public partial interface IWidgetPlugin : IPlugin
    {
        IList<string> GetWidgetZones();

        void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues);
    }
}
