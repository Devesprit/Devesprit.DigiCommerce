using System;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.Helpers;
using Elmah;
using Syncfusion.JavaScript;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManagePluginsController : BaseController
    {
        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public ManagePluginsController(IPluginFinder pluginFinder,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _pluginFinder = pluginFinder;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult GridDataSource(DataManager dm)
        {
            var dataSource = _pluginFinder.GetPluginDescriptors(LoadPluginsMode.All).ToList().Select(p => new
            {
                Info = "<small><b>" + _localizationService.GetResource("Author") + ": </b>" + p.Author + "<br/>" +
                       "<b>" + _localizationService.GetResource("SystemName") + ": </b>" + p.SystemName + "<br/>" +
                       "<b>" + _localizationService.GetResource("Version") + ": </b>" + p.Version + "<br/>" +
                       "<b>" + _localizationService.GetResource("Installed") + ": </b>" + (p.Installed
                           ? "<i class=\"fa fa-check\" aria-hidden=\"true\"></i>"
                           : "<i class=\"fa fa-times\" aria-hidden=\"true\"></i>") + "</small>",
                p.Installed,
                Name = "<b>" + p.FriendlyName + "</b><br/><small><b>" + _localizationService.GetResource("Description") + ": </b><br/>" + p.Description+ "</small>",
                LogoUrl = p.GetLogoUrl(),
                p.SystemName,
                p.Group,
                p.DisplayOrder
            });


            var result = dataSource.AsQueryable().ApplyDataManager(dm, out var count).ToList();
            return Json(dm.RequiresCounts ? new { result = result, count = count } : (object)result,
                JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Update(PluginDescriptor value)
        {
            var plugin = _pluginFinder.GetPluginDescriptorBySystemName(value.SystemName, LoadPluginsMode.All);
            plugin.DisplayOrder = value.DisplayOrder;
            PluginFileParser.SavePluginDescriptionFile(plugin);
            return Json(value, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult Install(string pluginName)
        {
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pluginName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("Index");

                //check whether plugin is not installed
                if (pluginDescriptor.Installed)
                    return RedirectToAction("Index");

                //install plugin
                pluginDescriptor.Instance().Install();

                _webHelper.RestartAppDomain();
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        public virtual ActionResult Uninstall(string pluginName)
        {
            try
            {
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName(pluginName, LoadPluginsMode.All);
                if (pluginDescriptor == null)
                    //No plugin found with the specified id
                    return RedirectToAction("Index");

                //check whether plugin is installed
                if (!pluginDescriptor.Installed)
                    return RedirectToAction("Index");

                //uninstall plugin
                pluginDescriptor.Instance().Uninstall();

                //restart application
                _webHelper.RestartAppDomain();
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public virtual ActionResult ConfigPlugin(string pluginName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(pluginName, LoadPluginsMode.InstalledOnly);
            if (descriptor == null || !descriptor.Installed)
                return RedirectToAction("PageNotFound", "Error");

            var plugin = descriptor.Instance<IPlugin>();

            plugin.GetConfigurationRoute(out var actionName, out var controllerName, out var routeValues);
            var model = new ConfigPluginModel
            {
                FriendlyName = descriptor.FriendlyName,
                ConfigurationActionName = actionName,
                ConfigurationControllerName = controllerName,
                ConfigurationRouteValues = routeValues
            };
            return View(model);
        }

        public virtual ActionResult ReloadList()
        {
            _webHelper.RestartAppDomain();
            return RedirectToAction("Index");
        }
    }
}