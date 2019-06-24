using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Devesprit.Core.Plugin
{
    public static partial class PluginExtensions
    {

        private static readonly List<string> SupportedLogoImageExtensions = new List<string>
        {
            "jpg",
            "png",
            "gif"
        };

        public static string GetLogoUrl(this PluginDescriptor pluginDescriptor)
        {
            if (pluginDescriptor == null)
                throw new ArgumentNullException(nameof(pluginDescriptor));
            

            if (pluginDescriptor.OriginalAssemblyFile == null || pluginDescriptor.OriginalAssemblyFile.Directory == null)
            {
                return null;
            }

            var pluginDirectory = pluginDescriptor.OriginalAssemblyFile.Directory;

            var logoExtension = SupportedLogoImageExtensions.FirstOrDefault(ext => File.Exists(Path.Combine(pluginDirectory.FullName, "logo." + ext)));

            if (string.IsNullOrWhiteSpace(logoExtension)) return null; //No logo file was found with any of the supported extensions.

            string logoUrl = string.Format("/plugins/{0}/logo.{1}", pluginDirectory.Name, logoExtension);
            return logoUrl;
        }

    }
}
