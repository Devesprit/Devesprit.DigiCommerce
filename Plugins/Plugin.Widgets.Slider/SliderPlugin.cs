using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Devesprit.Core.Plugin;
using Devesprit.Data.Domain;
using Devesprit.Services.Localization;
using Devesprit.Services.Users;
using Devesprit.Services.Widget;
using Plugin.Widgets.Slider.DB;
using Z.EntityFramework.Plus;

namespace Plugin.Widgets.Slider
{
    public partial class SliderPlugin : BasePlugin, IWidgetPlugin
    {
        internal const string CacheKey = "Plugin.Widgets.Slider";
        private readonly SliderDbContext _dbContext;
        private readonly IUserRolesService _userRolesService;

        public SliderPlugin(SliderDbContext dbContext, IUserRolesService userRolesService)
        {
            _dbContext = dbContext;
            _userRolesService = userRolesService;
        }
         
        public virtual IList<string> GetWidgetZones()
        {
            var zoneLists = _dbContext.Slider.Select(p => p.Zone).FromCache(CacheKey).Distinct().ToList();
            return zoneLists;
        }

        public virtual void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "WidgetsSlider";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.Widgets.Slider.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsSlider";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.Widgets.Slider.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.DisplayZone", "Display Zone", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.DestinationURL", "Destination URL", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.LinkTarget", "Target", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.OnClickJS", "OnClick Java Script", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.ManageImageSlider", "Manage Image Slider", "en");
            

            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.DisplayZone", "ناحیه نمایش", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.DestinationURL", "آدرس URL مقصد", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.LinkTarget", "Target", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.OnClickJS", "OnClick Java Script", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.Slider.ManageImageSlider", "مدیریت اسلایدر تصاویر", "fa");

            _userRolesService.AddAccessAreas(new TblUserAccessAreas("Plugins", "DevespritImageSliderConfig", "Plugin.Widgets.Slider.ManageImageSlider"));
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Plugin.Widgets.Slider.DisplayZone");
            this.DeletePluginLocaleResource("Plugin.Widgets.Slider.DestinationURL");
            this.DeletePluginLocaleResource("Plugin.Widgets.Slider.LinkTarget");
            this.DeletePluginLocaleResource("Plugin.Widgets.Slider.OnClickJS");
            this.DeletePluginLocaleResource("Plugin.Widgets.Slider.ManageImageSlider");

            _userRolesService.DeleteAccessAreas("DevespritImageSliderConfig");
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Uninstall();
        }
    }
}
