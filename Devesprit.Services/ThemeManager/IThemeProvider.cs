using System.Collections.Generic;

namespace Devesprit.Services.ThemeManager
{
    public partial interface IThemeProvider
    {
        ThemeConfiguration GetThemeConfiguration(string themeName);

        IList<ThemeConfiguration> GetThemeConfigurations();

        void ReLoadInstalledThemes();

        bool ThemeConfigurationExists(string themeName);
    }
}
