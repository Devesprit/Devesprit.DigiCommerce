using System;
using System.Collections.Generic;
using System.Linq;
using Devesprit.Core.Plugin;

namespace Devesprit.Services.Widget
{
    public partial class WidgetService : IWidgetService
    {
        private readonly IPluginFinder _pluginFinder;
        
        public WidgetService(IPluginFinder pluginFinder)
        {
            this._pluginFinder = pluginFinder;
        }
        
        public virtual IList<IWidgetPlugin> LoadWidgetsByWidgetZone(string widgetZone)
        {
            if (string.IsNullOrWhiteSpace(widgetZone))
                return new List<IWidgetPlugin>();

            return LoadAllWidgets()
                .Where(x => x.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase))
                .ToList();
        }

        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            return descriptor?.Instance<IWidgetPlugin>();
        }

        public virtual IList<IWidgetPlugin> LoadAllWidgets()
        {
            return _pluginFinder.GetPlugins<IWidgetPlugin>().ToList();
        }
    }
}