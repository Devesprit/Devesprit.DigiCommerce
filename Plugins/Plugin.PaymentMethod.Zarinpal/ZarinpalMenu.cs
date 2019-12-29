using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Services.AdminAreaMenu;

namespace Plugin.PaymentMethod.Zarinpal
{
    public partial class ZarinpalMenu: IAdminAreaPluginMenu
    {
        public virtual List<AdminMenuItem> GetMenuItems(HttpContext ctx, ILocalizationService localizationService, UrlHelper url)
        {
            return new List<AdminMenuItem>()
            {
                new AdminMenuItem()
                {
                    DisplayOrder = 1,
                    MenuDisplayName = localizationService.GetResource("Plugin.PaymentMethod.Zarinpal.ZarinpalWebSite"),
                    DestUrl = "https://www.zarinpal.com/",
                    Target = "_blank",
                    Icon = "fa fa-external-link",
                    NeedPermission = "ZarinpalGateWayConfig"
                }
            };
        }
    }
}
