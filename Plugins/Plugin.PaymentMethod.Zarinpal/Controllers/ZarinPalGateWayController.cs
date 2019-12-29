using System;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.WebFramework.ActionFilters;
using Elmah;
using Plugin.PaymentMethod.Zarinpal.Models;

namespace Plugin.PaymentMethod.Zarinpal.Controllers
{
    [UserHasPermission("ZarinpalGateWayConfig")]
    public partial class ZarinPalGateWayController : BaseController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public ZarinPalGateWayController(ILocalizationService localizationService, ISettingService settingService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
        }

        [Authorize(Roles = "Admin")]
        public virtual ActionResult Configure()
        {
            var model = _settingService.LoadSetting<ZarinPalSettingsModel>();
            return View("~/Plugins/Plugin.PaymentMethod.Zarinpal/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Configure(ZarinPalSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Plugins/Plugin.PaymentMethod.Zarinpal/Views/Configure.cshtml", model);
            }

            try
            {
                _settingService.SaveSetting(model);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View("~/Plugins/Plugin.PaymentMethod.Zarinpal/Views/Configure.cshtml", model);
            }
            
            return Content(@"<script language='javascript' type='text/javascript'>
                                window.close();
                             </script>");
        }
    }
}
