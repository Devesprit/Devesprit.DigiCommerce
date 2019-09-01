using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using Devesprit.Core.Plugin;

[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]
namespace Devesprit.Core.Plugin
{
    public partial class PluginManager
    {
        private const string InstalledPluginsFilePath = "~/App_Data/InstalledPlugins.txt";
        private const string PluginsPath = "~/Plugins";
        private const string ShadowCopyPath = "~/Plugins/bin";
        
        private static readonly ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();
        private static DirectoryInfo _shadowCopyFolder;
        private static bool _clearShadowDirectoryOnStartup;

        public static IEnumerable<PluginDescriptor> ReferencedPlugins { get; set; }
        

        public static void Initialize()
        {
            using (new WriteLockDisposable(Locker))
            {
                // prevent app from starting altogether
                var pluginFolder = new DirectoryInfo(CommonHelper.MapPath(PluginsPath));
                _shadowCopyFolder = new DirectoryInfo(CommonHelper.MapPath(ShadowCopyPath));

                var referencedPlugins = new List<PluginDescriptor>();
                
                _clearShadowDirectoryOnStartup = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["ClearPluginsShadowDirectoryOnStartup"]) &&
                                                 Convert.ToBoolean(ConfigurationManager.AppSettings["ClearPluginsShadowDirectoryOnStartup"]);

                try
                {
                    var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());

                    Debug.WriteLine("Creating shadow copy folder and querying for dlls");
                    //ensure folders are created
                    Directory.CreateDirectory(pluginFolder.FullName);
                    Directory.CreateDirectory(_shadowCopyFolder.FullName);

                    //get list of all files in bin
                    var binFiles = _shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);

                    //clear out shadow copied plugins
                    if (_clearShadowDirectoryOnStartup)
                    {
                        foreach (var f in binFiles)
                        {
                            Debug.WriteLine("Deleting " + f.Name);
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch (Exception exc)
                            {
                                Debug.WriteLine("Error deleting file " + f.Name + ". Exception: " + exc);
                            }
                        }
                    }

                    //load description files
                    foreach (var dfd in GetDescriptionFilesAndDescriptors(pluginFolder))
                    {
                        var descriptionFile = dfd.Key;
                        var pluginDescriptor = dfd.Value;
                        
                        //some validation
                        if (String.IsNullOrWhiteSpace(pluginDescriptor.SystemName))
                            throw new Exception(string.Format("A plugin '{0}' has no system name. Try assigning the plugin a unique name and recompiling.", descriptionFile.FullName));
                        if (referencedPlugins.Contains(pluginDescriptor))
                            throw new Exception(string.Format("A plugin with '{0}' system name is already defined", pluginDescriptor.SystemName));

                        //set 'Installed' property
                        pluginDescriptor.Installed = installedPluginSystemNames
                            .FirstOrDefault(x => x.Equals(pluginDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase)) != null;

                        try
                        {
                            if (descriptionFile.Directory == null)
                                throw new Exception(string.Format("Directory cannot be resolved for '{0}' description file", descriptionFile.Name));
                            //get list of all DLLs in plugins (not in bin!)
                            var pluginFiles = descriptionFile.Directory.GetFiles("*.dll", SearchOption.AllDirectories)
                                //just make sure we're not registering shadow copied plugins
                                .Where(x => !binFiles.Select(q => q.FullName).Contains(x.FullName))
                                .Where(x => IsPackagePluginFolder(x.Directory))
                                .ToList();

                            //other plugin description info
                            var mainPluginFile = pluginFiles
                                .FirstOrDefault(x => x.Name.Equals(pluginDescriptor.PluginFileName, StringComparison.InvariantCultureIgnoreCase));
                            pluginDescriptor.OriginalAssemblyFile = mainPluginFile;

                            //shadow copy main plugin file
                            pluginDescriptor.ReferencedAssembly = PerformFileDeploy(mainPluginFile);

                            //load all other referenced assemblies now
                            foreach (var plugin in pluginFiles
                                .Where(x => !x.Name.Equals(mainPluginFile.Name, StringComparison.InvariantCultureIgnoreCase))
                                .Where(x => !IsAlreadyLoaded(x)))
                                PerformFileDeploy(plugin);

                            //init plugin type (only one plugin per assembly is allowed)
                            foreach (var t in pluginDescriptor.ReferencedAssembly.GetExportedTypes())
                                if (typeof(IPlugin).IsAssignableFrom(t))
                                    if (!t.IsInterface)
                                        if (t.IsClass && !t.IsAbstract)
                                        {
                                            pluginDescriptor.PluginType = t;
                                            break;
                                        }

                            referencedPlugins.Add(pluginDescriptor);
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            //add a plugin name. this way we can easily identify a problematic plugin
                            var msg = $"Plugin '{pluginDescriptor.FriendlyName}'. ";
                            foreach (var e in ex.LoaderExceptions)
                                msg += e.Message + Environment.NewLine;

                            var fail = new Exception(msg, ex);
                            throw fail;
                        }
                        catch (Exception ex)
                        {
                            //add a plugin name. this way we can easily identify a problematic plugin
                            var msg = $"Plugin '{pluginDescriptor.FriendlyName}'. {ex.Message}";

                            var fail = new Exception(msg, ex);
                            throw fail;
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = string.Empty;
                    for (var e = ex; e != null; e = e.InnerException)
                        msg += e.Message + Environment.NewLine;

                    var fail = new Exception(msg, ex);
                    throw fail;
                }


                ReferencedPlugins = referencedPlugins;
            }
        }
        
        public static void MarkPluginAsInstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            if (!alreadyMarkedAsInstalled)
                installedPluginSystemNames.Add(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }
        
        public static void MarkPluginAsUninstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            if (alreadyMarkedAsInstalled)
                installedPluginSystemNames.Remove(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }

        public static void MarkAllPluginsAsUninstalled()
        {
            var filePath = CommonHelper.MapPath(InstalledPluginsFilePath);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public static PluginDescriptor FindPluginByType(Type providerType)
        {
            if (providerType == null)
                throw new ArgumentNullException(nameof(providerType));

            foreach (var plugin in ReferencedPlugins)
            {
                if (plugin.ReferencedAssembly == null)
                    continue;

                if (plugin.ReferencedAssembly.FullName == providerType.Assembly.FullName)
                    return plugin;
            }

            return null;
        }

        #region Utilities

        protected static IEnumerable<KeyValuePair<FileInfo, PluginDescriptor>> GetDescriptionFilesAndDescriptors(DirectoryInfo pluginFolder)
        {
            if (pluginFolder == null)
                throw new ArgumentNullException(nameof(pluginFolder));

            var result = new List<KeyValuePair<FileInfo, PluginDescriptor>>();
            foreach (var descriptionFile in pluginFolder.GetFiles("Description.txt", SearchOption.AllDirectories))
            {
                if (!IsPackagePluginFolder(descriptionFile.Directory))
                    continue;

                //parse file
                var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);

                //populate list
                result.Add(new KeyValuePair<FileInfo, PluginDescriptor>(descriptionFile, pluginDescriptor));
            }

            result.Sort((firstPair, nextPair) => firstPair.Value.DisplayOrder.CompareTo(nextPair.Value.DisplayOrder));
            return result;
        }

        protected static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = a.FullName.Split(new[] { ',' }).FirstOrDefault();
                    if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + exc);
            }
            return false;
        }

        protected static Assembly PerformFileDeploy(FileInfo plug)
        {
            if (plug.Directory == null || plug.Directory.Parent == null)
                throw new InvalidOperationException("The plugin directory for the " + plug.Name + " file exists in a folder outside of the allowed DigiCommerce folder hierarchy");

            FileInfo shadowCopiedPlug;

            if (CommonHelper.GetTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
            {
                var shadowCopyPlugFolder = Directory.CreateDirectory(_shadowCopyFolder.FullName);
                shadowCopiedPlug = InitializeMediumTrust(plug, shadowCopyPlugFolder);
            }
            else
            {
                var directory = AppDomain.CurrentDomain.DynamicDirectory;
                Debug.WriteLine(plug.FullName + " to " + directory);
                shadowCopiedPlug = InitializeFullTrust(plug, new DirectoryInfo(directory));
            }

            var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));

            Debug.WriteLine("Adding to BuildManager: '{0}'", shadowCopiedAssembly.FullName);
            BuildManager.AddReferencedAssembly(shadowCopiedAssembly);

            return shadowCopiedAssembly;
        }

        protected static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                
                try
                {
                    var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    File.Move(shadowCopiedPlug.FullName, oldFile);
                }
                catch (IOException exc)
                {
                    throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                }

                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }

        protected static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            if (shadowCopiedPlug.Exists)
            {
                var areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
                else
                {
                    Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                    File.Delete(shadowCopiedPlug.FullName);
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    
                    try
                    {
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException exc)
                    {
                        throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                    }
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
        }

        protected static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder?.Parent == null) return false;
            if (!folder.Parent.Name.Equals("Plugins", StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }

        protected static string GetInstalledPluginsFilePath()
        {
            return CommonHelper.MapPath(InstalledPluginsFilePath);
        }

        #endregion
    }
}
