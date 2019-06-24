using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.Services.AdminAreaMenu
{
    public partial interface IAdminAreaMenuManager
    {
        List<AdminMenuItem> LoadAllPluginsMenu(HttpContext httpContext, ILocalizationService localizationService, UrlHelper url);
    }
}
