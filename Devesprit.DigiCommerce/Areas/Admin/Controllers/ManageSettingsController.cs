using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.Services;
using Devesprit.Services.SearchEngine;
using Elmah;
using Hangfire;

namespace Devesprit.DigiCommerce.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public partial class ManageSettingsController : BaseController
    {
        private readonly ISettingService _settingService;
        private readonly ISettingsModelFactory _settingsModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public ManageSettingsController(ISettingService settingService,
            ISettingsModelFactory settingsModelFactory,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _settingService = settingService;
            _settingsModelFactory = settingsModelFactory;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }
        
        public virtual async Task<ActionResult> Index()
        {
            var record = await _settingService.LoadSettingAsync<SiteSettings>();
            return View(_settingsModelFactory.PrepareSiteSettingModel(record));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Index(SiteSettingModel model)
        {
            #region Validations
            if (model.UseGoogleRecaptchaForLogin || model.UseGoogleRecaptchaForResetPassword || model.UseGoogleRecaptchaForSignup || model.UseGoogleRecaptchaForComment)
            {
                if (string.IsNullOrWhiteSpace(model.GoogleRecaptchaSecretKey))
                {
                    ModelState.AddModelError("", string.Format(_localizationService.GetResource("FieldRequired"), _localizationService.GetResource("GoogleRecaptchaSecretKey")));
                }
                if (string.IsNullOrWhiteSpace(model.GoogleRecaptchaSiteKey))
                {
                    ModelState.AddModelError("", string.Format(_localizationService.GetResource("FieldRequired"), _localizationService.GetResource("GoogleRecaptchaSiteKey")));
                }
            }
            #endregion


            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            var record = _settingsModelFactory.PrepareSiteSettings(model);
            
            try
            {
                await _settingService.SaveSettingAsync(record);


                //Update System EncryptionKey & EncryptionSalt
                var objConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                var objAppSettings = (AppSettingsSection)objConfig.GetSection("appSettings");
                if (objAppSettings != null)
                {
                    objAppSettings.Settings["EncryptionKey"].Value = model.EncryptionKey;
                    objAppSettings.Settings["EncryptionSalt"].Value = model.EncryptionSalt;
                    objConfig.Save();
                }

                //Update CKEditor settings
                System.IO.File.WriteAllText(HttpContext.Server.MapPath("~/Scripts/CKEditor/config.js"), model.CkEditorConfig);
                System.IO.File.WriteAllText(HttpContext.Server.MapPath("~/Scripts/CKEditor/Plugins/templates/templates/default.js"), model.CkEditorTemplates);
            }
            catch (Exception e)
            {
                var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(e, System.Web.HttpContext.Current));
                ModelState.AddModelError("", string.Format(_localizationService.GetResource("ErrorOnOperation"), e.Message, errorCode));
                return View(model);
            }

            _webHelper.RestartAppDomain();

            return RedirectToAction("Index");
        }

        public virtual ActionResult ClearApplicationErrorLog()
        {
            try
            {
                System.IO.File.Delete(Server.MapPath("~/App_Data/errors.s3db"));
                SuccessNotification(_localizationService.GetResource("ErrorsLogCleared"));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
            }
            
            return RedirectToAction("Index", "Administration");
        }

        public virtual ActionResult RefreshSearchEngineIndexes()
        {
            try
            {
                BackgroundJob.Enqueue<ISearchEngine>(searchEngine => searchEngine.CreateIndex());
                SuccessNotification(_localizationService.GetResource("OperationCompletedSuccessfully"));
            }
            catch (Exception e)
            {
                ErrorNotification(e);
            }

            return RedirectToAction("Index", "Administration");
        }
    }
}