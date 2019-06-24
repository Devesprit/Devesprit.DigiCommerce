using System;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services.Localization;
using Elmah;
using Plugin.Widgets.GoogleAnalytics.Models;

namespace Plugin.Widgets.GoogleAnalytics.Controllers
{
    public partial class WidgetsGoogleAnalyticsController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public WidgetsGoogleAnalyticsController(ILocalizationService localizationService, ISettingService settingService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult Configure()
        {
            var model = _settingService.LoadSetting<GoogleAnalyticsSettingsModel>();
            return View("~/Plugins/Plugin.Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Configure(GoogleAnalyticsSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Plugins/Plugin.Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
            }

            try
            {
                _settingService.SaveSetting(model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View("~/Plugins/Plugin.Widgets.GoogleAnalytics/Views/Configure.cshtml", model);
            }
            
            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                             </script>");
        }

        [ChildActionOnly]
        public virtual ActionResult Index(string widgetZone, object additionalData = null)
        {
            var settings = _settingService.LoadSetting<GoogleAnalyticsSettingsModel>();

            string globalScript = settings.GetLocalized(x=> x.TrackingScript).Replace("{GOOGLEID}", settings.GetLocalized(x => x.GoogleId));
            var routeData = ((System.Web.UI.Page)this.HttpContext.CurrentHandler).RouteData;

            var controller = routeData.Values["controller"];
            var action = routeData.Values["action"];

            if (controller == null || action == null)
                return Content("");
            
            return Content(globalScript);
        }
    }
}
