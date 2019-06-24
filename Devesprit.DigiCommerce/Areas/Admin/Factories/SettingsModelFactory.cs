using System;
using System.Configuration;
using System.IO;
using System.Web;
using AutoMapper;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services;

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
                result = Mapper.Map<SiteSettingModel>(setting);
                result.AccountLockoutTime = (int) setting.AccountLockoutTimeSpan.TotalMinutes;
            }

            Configuration objConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            AppSettingsSection objAppSettings = (AppSettingsSection)objConfig.GetSection("appSettings");

            result.EncryptionKey = objAppSettings.Settings["EncryptionKey"].Value;
            result.EncryptionSalt = objAppSettings.Settings["EncryptionSalt"].Value;

            if (File.Exists(_httpContext.Server.MapPath("~/Scripts/CKEditor/config.js")))
            {
                result.CkEditorConfig = File.ReadAllText(_httpContext.Server.MapPath("~/Scripts/CKEditor/config.js"));
            }
            if (File.Exists(_httpContext.Server.MapPath("~/Scripts/CKEditor/Plugins/templates/templates/default.js")))
            {
                result.CkEditorTemplates = File.ReadAllText(_httpContext.Server.MapPath("~/Scripts/CKEditor/Plugins/templates/templates/default.js"));
            }

            return result;
        }

        public virtual SiteSettings PrepareSiteSettings(SiteSettingModel setting)
        {
            var result = Mapper.Map<SiteSettings>(setting);
            result.AccountLockoutTimeSpan = TimeSpan.FromMinutes(setting.AccountLockoutTime);
            return result;
        }
    }
}