using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.Core.Localization;
using Devesprit.Services.AdminAreaMenu;

namespace Plugin.DiscountCode
{
    public partial class DiscountCodeAdminAreaMenu: IAdminAreaPluginMenu
    {
        public virtual List<AdminMenuItem> GetMenuItems(HttpContext ctx, ILocalizationService localizationService, UrlHelper url)
        {
            return new List<AdminMenuItem>()
            {
                new AdminMenuItem()
                {
                    DisplayOrder = 1,
                    MenuDisplayName = localizationService.GetResource("Plugin.DiscountCode.ManageDiscountCodes"),
                    DestUrl = url.Action("ConfigPlugin", "ManagePlugins", new RouteValueDictionary
                    {
                        {"pluginName", "DiscountProcessor.DiscountCode"},
                        { "area", "Admin"}
                    }),
                    Icon = "fa fa-tags"
                }
            };
        }
    }
}
