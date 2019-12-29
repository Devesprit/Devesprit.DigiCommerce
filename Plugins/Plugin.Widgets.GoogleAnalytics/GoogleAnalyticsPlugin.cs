using System.Collections.Generic;
using System.Web.Routing;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.Services.Localization;
using Devesprit.Services.Users;
using Devesprit.Services.Widget;
using Plugin.Widgets.GoogleAnalytics.Models;

namespace Plugin.Widgets.GoogleAnalytics
{
    public partial class GoogleAnalyticsPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IUserRolesService _userRolesService;

        public GoogleAnalyticsPlugin(ISettingService settingsService, IUserRolesService userRolesService)
        {
            _settingService = settingsService;
            _userRolesService = userRolesService;
        }

        public virtual IList<string> GetWidgetZones()
        {
            var settings = _settingService.LoadSetting<GoogleAnalyticsSettingsModel>();
            return new List<string>() {settings.WidgetZone};
        }

        public virtual void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName,
            out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "WidgetsGoogleAnalytics";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.Widgets.GoogleAnalytics.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsGoogleAnalytics";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Plugin.Widgets.GoogleAnalytics.Controllers"},
                { "area", null}
            };
        }

        public override void Install()
        {
            _settingService.SaveSetting(new GoogleAnalyticsSettingsModel());

            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.GoogleId", "Google ID", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.TrackingScript", "Tracking Script", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.WidgetZone", "Widget Zone", "en");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.Configuration", "Google Analytics Configurations", "en");
            
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.GoogleId", "گوگل آی دی (Google ID)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.TrackingScript", "اسکریپت ردیابی (Tracking Script)", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.WidgetZone", "ناحیه ویجت", "fa");
            this.AddOrUpdatePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.Configuration", "تنظیمات Google Analytics", "fa");

            _userRolesService.AddAccessAreas(new TblUserAccessAreas("Plugins", "GoogleAnalyticsConfig", "Plugin.Widgets.GoogleAnalytics.Configuration"));
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Install();
        }

        public override void Uninstall()
        {
            _settingService.DeleteSetting<GoogleAnalyticsSettingsModel>();

            this.DeletePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.GoogleId");
            this.DeletePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.TrackingScript");
            this.DeletePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.WidgetZone");
            this.DeletePluginLocaleResource("Plugin.Widgets.GoogleAnalytics.Configuration");

            _userRolesService.DeleteAccessAreas("GoogleAnalyticsConfig");
            _userRolesService.GrantAllPermissionsToAdministrator();

            base.Uninstall();
        }
    }
}
