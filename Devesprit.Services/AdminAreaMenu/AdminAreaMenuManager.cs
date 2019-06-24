using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.Services.AdminAreaMenu
{
    public partial class AdminAreaMenuManager : IAdminAreaMenuManager
    {
        public virtual List<AdminMenuItem> LoadAllPluginsMenu(HttpContext httpContext, ILocalizationService localizationService, UrlHelper url)
        {
            var adminAreaPluginMenus = DependencyResolver.Current.GetServices<IAdminAreaPluginMenu>();
            var result = new List<AdminMenuItem>();
            foreach (var adminAreaPluginMenu in adminAreaPluginMenus)
            {
                result.AddRange(adminAreaPluginMenu.GetMenuItems(httpContext, localizationService, url).OrderBy(p=> p.DisplayOrder));
            }

            return result;
        }
    }
}