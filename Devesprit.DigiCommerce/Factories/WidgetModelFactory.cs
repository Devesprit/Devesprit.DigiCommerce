using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Factories.Interfaces;
using Devesprit.DigiCommerce.Models.Widget;
using Devesprit.Services;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.Widget;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce.Factories
{
    public partial class WidgetModelFactory : IWidgetModelFactory
    {
        private readonly ISettingService _settingService;
        private readonly IMemoryCache _memoryCache;
        private readonly IWidgetService _widgetService;
        private readonly HttpContextBase _httpContext;

        public WidgetModelFactory(ISettingService settingService, 
            IMemoryCache memoryCache, 
            IWidgetService widgetService,
            HttpContextBase httpContext)
        {
            _settingService = settingService;
            _memoryCache = memoryCache;
            _widgetService = widgetService;
            _httpContext = httpContext;
        }

        public virtual List<RenderWidgetModel> GetRenderWidgetModels(string widgetZone, object additionalData = null)
        {
            var currentUserId = _httpContext.User.Identity.GetUserId();
            var currentSettings = _settingService.LoadSetting<SiteSettings>();

            var cacheKey = $"widget-{currentUserId}-{widgetZone}-{currentSettings.WebsiteTheme}";
            
            var cachedModel = _memoryCache.Get(cacheKey, () =>
            {
                //model
                var model = new List<RenderWidgetModel>();

                var widgets = _widgetService.LoadWidgetsByWidgetZone(widgetZone);
                foreach (var widget in widgets)
                {
                    var widgetModel = new RenderWidgetModel();

                    widget.GetDisplayWidgetRoute(widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues);
                    widgetModel.ActionName = actionName;
                    widgetModel.ControllerName = controllerName;
                    widgetModel.RouteValues = routeValues;

                    model.Add(widgetModel);
                }
                return model;
            });

            //"RouteValues" property of widget models depends on "additionalData".
            //We need to clone the cached model before modifications (the updated one should not be cached)
            var clonedModel = new List<RenderWidgetModel>();
            foreach (var widgetModel in cachedModel)
            {
                var clonedWidgetModel = new RenderWidgetModel();
                clonedWidgetModel.ActionName = widgetModel.ActionName;
                clonedWidgetModel.ControllerName = widgetModel.ControllerName;
                if (widgetModel.RouteValues != null)
                    clonedWidgetModel.RouteValues = new RouteValueDictionary(widgetModel.RouteValues);

                if (additionalData != null)
                {
                    if (clonedWidgetModel.RouteValues == null)
                        clonedWidgetModel.RouteValues = new RouteValueDictionary();
                    clonedWidgetModel.RouteValues.Add("additionalData", additionalData);
                }

                clonedModel.Add(clonedWidgetModel);
            }

            return clonedModel;
        }
    }
}