using System;
using System.Collections.Generic;
using System.Linq;

namespace Devesprit.Core.Plugin
{
    public partial class PluginFinder : IPluginFinder
    {
        private IList<PluginDescriptor> _plugins;
        private bool _arePluginsLoaded;

        protected virtual void EnsurePluginsAreLoaded()
        {
            if (!_arePluginsLoaded)
            {
                var foundPlugins = PluginManager.ReferencedPlugins.ToList();
                foundPlugins.Sort();
                _plugins = foundPlugins.ToList();

                _arePluginsLoaded = true;
            }
        }

        protected virtual bool CheckLoadMode(PluginDescriptor pluginDescriptor, LoadPluginsMode loadMode)
        {
            if (pluginDescriptor == null)
                throw new ArgumentNullException(nameof(pluginDescriptor));

            switch (loadMode)
            {
                case LoadPluginsMode.All:
                    //no filtering
                    return true;
                case LoadPluginsMode.InstalledOnly:
                    return pluginDescriptor.Installed;
                case LoadPluginsMode.NotInstalledOnly:
                    return !pluginDescriptor.Installed;
                default:
                    throw new Exception("Not supported LoadPluginsMode");
            }
        }

        protected virtual bool CheckGroup(PluginDescriptor pluginDescriptor, string group)
        {
            if (pluginDescriptor == null)
                throw new ArgumentNullException(nameof(pluginDescriptor));

            if (String.IsNullOrEmpty(group))
                return true;

            return group.Equals(pluginDescriptor.Group, StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null)
        {
            //ensure plugins are loaded
            EnsurePluginsAreLoaded();

            return _plugins.Where(p => CheckLoadMode(p, loadMode) && CheckGroup(p, group));
        }

        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null)
            where T : class, IPlugin
        {
            return GetPluginDescriptors(loadMode, group)
                .Where(p => typeof(T).IsAssignableFrom(p.PluginType));
        }

        public virtual PluginDescriptor GetPluginDescriptorBySystemName(string systemName,
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly)
        {
            return GetPluginDescriptors(loadMode)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual IEnumerable<T> GetPlugins<T>(LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly, string group = null) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(loadMode, group).Select(p => p.Instance<T>());
        }

        public virtual PluginDescriptor GetPluginDescriptorBySystemName<T>(string systemName,
            LoadPluginsMode loadMode = LoadPluginsMode.InstalledOnly) where T : class, IPlugin
        {
            return GetPluginDescriptors<T>(loadMode)
                .SingleOrDefault(p => p.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void ReloadPlugins()
        {
            _arePluginsLoaded = false;
            EnsurePluginsAreLoaded();
        }
    }
}