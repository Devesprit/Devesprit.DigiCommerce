using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Devesprit.Services.ThemeManager
{
    public partial class ThemeProvider : IThemeProvider
    {
        private readonly IList<ThemeConfiguration> _themeConfigurations = new List<ThemeConfiguration>();
        private string _basePath;
        private static HttpContext HttpContext => HttpContext.Current;
        public ThemeProvider()
        {
            _basePath = HttpContext.Server.MapPath("~/Themes/");
            LoadConfigurations();
        }

        public virtual ThemeConfiguration GetThemeConfiguration(string themeName)
        {
            return _themeConfigurations
                .SingleOrDefault(x => x.ThemeName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual IList<ThemeConfiguration> GetThemeConfigurations()
        {
            return _themeConfigurations;
        }

        public virtual bool ThemeConfigurationExists(string themeName)
        {
            return GetThemeConfigurations().Any(configuration => configuration.ThemeName.Equals(themeName, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual void ReLoadInstalledThemes()
        {
            _basePath = HttpContext.Server.MapPath("~/Themes/");
            LoadConfigurations();
        }

        protected virtual void LoadConfigurations()
        {
            _themeConfigurations.Clear();
            foreach (string themeName in Directory.GetDirectories(_basePath))
            {
                var configuration = CreateThemeConfiguration(themeName);
                if (configuration != null)
                {
                    _themeConfigurations.Add(configuration);
                }
            }
        }

        protected virtual ThemeConfiguration CreateThemeConfiguration(string themePath)
        {
            var themeDirectory = new DirectoryInfo(themePath);
            var themeConfigFile = new FileInfo(Path.Combine(themeDirectory.FullName, "theme.config"));

            if (themeConfigFile.Exists)
            {
                var doc = new XmlDocument();
                doc.Load(themeConfigFile.FullName);
                return new ThemeConfiguration(themeDirectory.Name, themeDirectory.FullName, doc);
            }

            return null;
        }
    }
}
