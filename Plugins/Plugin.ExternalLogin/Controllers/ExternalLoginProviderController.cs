using System;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Controllers;
using Elmah;
using Plugin.ExternalLogin.Models;

namespace Plugin.ExternalLogin.Controllers
{
    public partial class ExternalLoginProviderController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public ExternalLoginProviderController(ILocalizationService localizationService, ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult Configure()
        {
            var model = _settingService.LoadSetting<ExternalLoginProviderSettingsModel>();
            return View("~/Plugins/Plugin.ExternalLogin/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Configure(ExternalLoginProviderSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Plugins/Plugin.ExternalLogin/Views/Configure.cshtml", model);
            }

            try
            {
                _settingService.SaveSetting(model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View("~/Plugins/Plugin.ExternalLogin/Views/Configure.cshtml", model);
            }

            _webHelper.RestartAppDomain();

            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                             </script>");
        }
    }
}
