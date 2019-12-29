using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.Core.Localization;
using Devesprit.Services.AdminAreaMenu;

namespace Plugin.Widgets.Slider
{
    public partial class SliderAdminAreaMenu: IAdminAreaPluginMenu
    {
        public virtual List<AdminMenuItem> GetMenuItems(HttpContext ctx, ILocalizationService localizationService, UrlHelper url)
        {
            return new List<AdminMenuItem>()
            {
                new AdminMenuItem()
                {
                    DisplayOrder = 1,
                    MenuDisplayName = localizationService.GetResource("Plugin.Widgets.Slider.ManageImageSlider"),
                    DestUrl = url.Action("ConfigPlugin", "ManagePlugins", new RouteValueDictionary
                    {
                        {"pluginName", "Widgets.Slider"},
                        { "area", "Admin"}
                    }),
                    Icon = "fa fa-picture-o",
                    NeedPermission = "DevespritImageSliderConfig"
                }
            };
        }
    }
}
