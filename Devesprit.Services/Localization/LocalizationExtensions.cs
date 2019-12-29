using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Languages;

namespace Devesprit.Services.Localization
{
    public static partial class LocalizationExtensions
    {
        public static TPropType GetLocalized<T, TPropType>(this T entity,
            Expression<Func<T, TPropType>> keySelector, int languageId,
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!(keySelector.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");
            }

            TPropType result = default(TPropType);
            string resultStr = string.Empty;

            //load localized value
            string localeKeyGroup = typeof(T).Name;
            string localeKey = propInfo.Name;

            if (languageId > 0)
            {
                if (HttpContext.Current == null)
                {
                    //for BackgroundJob (To resolve services by Autofac, HttpContext must NOT be null)
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("", "http://tempuri.org", ""),
                        new HttpResponse(new StringWriter())
                    );
                }

                //ensure that we have at least two published languages
                bool loadLocalizedString = true;
                if (ensureTwoPublishedLanguages)
                {
                    var lService = DependencyResolver.Current.GetService<ILanguagesService>();
                    var totalPublishedLanguages = lService.GetAllLanguagesIsoList().Count;
                    loadLocalizedString = totalPublishedLanguages >= 2;
                }

                //localized value
                if (loadLocalizedString)
                {
                    var leService = DependencyResolver.Current.GetService<ILocalizedEntityService>();
                    resultStr = leService.GetLocalizedString(languageId, entity.Id, localeKeyGroup, localeKey);
                    if (!String.IsNullOrEmpty(resultStr))
                        result = resultStr.To<TPropType>();
                }
            }

            //set default value if required
            if (String.IsNullOrEmpty(resultStr) && returnDefaultValue)
            {
                var localizer = keySelector.Compile();
                result = localizer(entity);
            }

            return result;
        }

        public static string GetLocalized<T>(this T entity,
            Expression<Func<T, string>> keySelector)
            where T : BaseEntity
        {
            var workContext = DependencyResolver.Current.GetService<IWorkContext>();
            return GetLocalized(entity, keySelector, workContext.CurrentLanguage.Id);
        }

        public static string GetLocalized<T>(this T entity,
            Expression<Func<T, string>> keySelector, int languageId,
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T : BaseEntity
        {
            return GetLocalized<T, string>(entity, keySelector, languageId, returnDefaultValue,
                ensureTwoPublishedLanguages);
        }

        public static async Task LoadAllLocalizedStringsToModelAsync<T>(this T entity,
            object model)
            where T : BaseEntity
        {
            var leService = DependencyResolver.Current.GetService<ILocalizedEntityService>();

            foreach (var property in model.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(LocalizedString)))
            {
                var result = new LocalizedString
                {
                    {0, (string) entity.GetPropertyValue(property.Name)} //default value
                };
                var locals = await leService.GetLocalizedPropertiesAsync(entity.Id, typeof(T).Name,
                    property.Name);
                foreach (var value in locals)
                {
                    result.Add(value.LanguageId, value.LocaleValue);
                }

                property.SetValue(model, result);
            }
        }

        public static string GetLocalized(this Enum value)
        {
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            return localization.GetResource(value.GetCustomAttributeDescription()).ToTitleCase();
        }

        #region Plugins

        public static void DeletePluginLocaleResource(this BasePlugin plugin,
            string resourceName)
        {
            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            var languageService = DependencyResolver.Current.GetService<ILanguagesService>();
            DeletePluginLocaleResource(plugin, localizationService,
                languageService, resourceName);
        }

        public static void DeletePluginLocaleResource(this BasePlugin plugin,
            ILocalizationService localizationService, ILanguagesService languageService,
            string resourceName)
        {
            //actually plugin instance is not required
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));
            if (languageService == null)
                throw new ArgumentNullException(nameof(languageService));

            foreach (var lang in languageService.GetAsEnumerable())
            {
                var lsr = localizationService.FindByName(resourceName, lang.Id);
                if (lsr != null)
                    localizationService.Delete(lsr.Id);
            }
        }

        public static void AddOrUpdatePluginLocaleResource(this BasePlugin plugin,
            string resourceName, string resourceValue, string languageISO = null)
        {
            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            var languageService = DependencyResolver.Current.GetService<ILanguagesService>();
            AddOrUpdatePluginLocaleResource(plugin, localizationService,
                languageService, resourceName, resourceValue, languageISO);
        }

        public static void AddOrUpdatePluginLocaleResource(this BasePlugin plugin,
            ILocalizationService localizationService, ILanguagesService languageService,
            string resourceName, string resourceValue, string languageISO = null)
        {
            //actually plugin instance is not required
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            if (localizationService == null)
                throw new ArgumentNullException(nameof(localizationService));
            if (languageService == null)
                throw new ArgumentNullException(nameof(languageService));

            foreach (var lang in languageService.GetAsEnumerable())
            {
                if (!string.IsNullOrEmpty(languageISO) && !languageISO.Equals(lang.IsoCode))
                    continue;

                var lsr = localizationService.FindByName(resourceName, lang.Id);
                if (lsr == null)
                {
                    lsr = new TblLocalizedStrings()
                    {
                        LanguageId = lang.Id,
                        ResourceName = resourceName,
                        ResourceValue = resourceValue
                    };
                    localizationService.Add(lsr);
                }
                else
                {
                    lsr.ResourceValue = resourceValue;
                    localizationService.Update(lsr);
                }
            }
        }

        #endregion


        #region Settings

        public static string GetLocalized<T>(this T setting,
            Expression<Func<T, LocalizedString>> keySelector, int languageId,
            bool returnDefaultValue = true, bool ensureTwoPublishedLanguages = true)
            where T : ISettings
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (!(keySelector.Body is MemberExpression member))
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");
            }

            string result = string.Empty;

            //load localized value
            string localeKeyGroup = typeof(T).Name;
            string localeKey = propInfo.Name;

            if (languageId > 0)
            {
                if (HttpContext.Current == null)
                {
                    //for BackgroundJob (To resolve services by Autofac, HttpContext must NOT be null)
                    HttpContext.Current = new HttpContext(
                        new HttpRequest("", "http://tempuri.org", ""),
                        new HttpResponse(new StringWriter())
                    );
                }

                //ensure that we have at least two published languages
                bool loadLocalizedString = true;
                if (ensureTwoPublishedLanguages)
                {
                    var lService = DependencyResolver.Current.GetService<ILanguagesService>();
                    var totalPublishedLanguages = lService.GetAllLanguagesIsoList().Count;
                    loadLocalizedString = totalPublishedLanguages >= 2;
                }

                //localized value
                if (loadLocalizedString)
                {
                    var leService = DependencyResolver.Current.GetService<ILocalizedEntityService>();
                    result = leService.GetLocalizedString(languageId, 0, localeKeyGroup, localeKey);
                }
            }

            //set default value if required
            if (string.IsNullOrEmpty(result) && returnDefaultValue)
            {
                var localizer = keySelector.Compile();
                if (localizer(setting)?.ContainsKey(0) ?? false)
                {
                    result = localizer(setting)[0];
                }
                else
                {
                    result = string.Empty;
                }
            }

            return result;
        }

        public static string GetLocalized<T>(this T entity,
            Expression<Func<T, LocalizedString>> keySelector)
            where T : ISettings
        {
            var workContext = DependencyResolver.Current.GetService<IWorkContext>();
            return GetLocalized(entity, keySelector, workContext.CurrentLanguage.Id);
        }

        #endregion
    }
}
