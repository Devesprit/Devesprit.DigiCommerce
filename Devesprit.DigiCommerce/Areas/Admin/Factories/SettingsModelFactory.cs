using System;
using System.Configuration;
using System.Web;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class SettingsModelFactory : ISettingsModelFactory
    {
        private readonly HttpContextBase _httpContext;

        public SettingsModelFactory(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public virtual SiteSettingModel PrepareSiteSettingModel(SiteSettings setting)
        {
            SiteSettingModel result;
            if (setting == null)
            {
                result = new SiteSettingModel();
            }
            else
            {
                result = setting.Adapt<SiteSettingModel>();
                result.AccountLockoutTime = (int) setting.AccountLockoutTimeSpan.TotalMinutes;
            }

            Configuration objConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            AppSettingsSection objAppSettings = (AppSettingsSection)objConfig.GetSection("appSettings");

            result.EncryptionKey = objAppSettings.Settings["EncryptionKey"].Value;
            result.EncryptionSalt = objAppSettings.Settings["EncryptionSalt"].Value;

            result.CacheLocalizedEntities =
                objAppSettings.Settings["CacheLocalizedEntities"].Value.ToBooleanOrDefault(true);

            return result;
        }

        public virtual SiteSettings PrepareSiteSettings(SiteSettingModel setting)
        {
            var result = setting.Adapt<SiteSettings>();
            result.AccountLockoutTimeSpan = TimeSpan.FromMinutes(setting.AccountLockoutTime);
            return result;
        }
    }
}