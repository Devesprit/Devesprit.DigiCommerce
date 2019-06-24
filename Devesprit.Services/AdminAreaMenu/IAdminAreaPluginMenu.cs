using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.Services.AdminAreaMenu
{
    public partial interface IAdminAreaPluginMenu
    {
        List<AdminMenuItem> GetMenuItems(HttpContext ctx, ILocalizationService localizationService, UrlHelper url);
    }
}
