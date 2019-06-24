using System.Collections.Generic;

namespace Devesprit.Core.Plugin
{
    public partial interface IPluginFinder
    {
        IEnumerable<T> GetPlugins<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null) where T : class, IPlugin;

        IEnumerable<PluginDescriptor> GetPluginDescriptors(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null);

        IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null) where T : class, IPlugin;

        PluginDescriptor GetPluginDescriptorBySystemName(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly);

        PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName, LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
            where T : class, IPlugin;

        void ReloadPlugins();
    }
}
