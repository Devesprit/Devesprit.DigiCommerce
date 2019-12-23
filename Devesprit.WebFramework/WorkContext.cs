using System;
using System.Linq;
using System.Web;
using Devesprit.Core;
using Devesprit.Data.Domain;
using Devesprit.Services.Currency;
using Devesprit.Services.Languages;
using Devesprit.Services.Users;
using Microsoft.AspNet.Identity;

namespace Devesprit.WebFramework
{
    public partial class WorkContext : IWorkContext
    {
        private HttpContext HttpContext { get; } = HttpContext.Current;

        private readonly ILanguagesService _languagesService;
        private readonly ICurrencyService _currencyService;
        private readonly IUserRolesService _userRolesService;
        private readonly IUsersService _usersService;

        public WorkContext(ILanguagesService languagesService, 
            ICurrencyService currencyService,
            IUserRolesService userRolesService,
            IUsersService usersService)
        {
            _languagesService = languagesService;
            _currencyService = currencyService;
            _userRolesService = userRolesService;
            _usersService = usersService;
        }

        protected virtual TblLanguages GetLanguageFromUrl()
        {
            if (HttpContext?.Request?.Url == null)
                return null;

            var currentPath = HttpContext?.Request.Url.PathAndQuery.TrimStart('/').Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            TblLanguages language = null;
            if (currentPath.Length >= 1 && currentPath[0].Length == 2)
            {
                var languageIso = currentPath[0];
                language = _languagesService.FindByIso(languageIso.Trim().ToLower());
            }

            if (language != null && language.Published)
            {
                return language;
            }

            return null;
        }

        protected virtual TblLanguages GetLanguageFromSession()
        {
            if (HttpContext?.Session?["CurrentLanguageISO"] == null)
                return null;

            var languageIso = HttpContext?.Session["CurrentLanguageISO"].ToString();
            var language = _languagesService.FindByIso(languageIso.Trim().ToLower());

            if (language != null && language.Published)
            {
                return language;
            }

            return null;
        }

        protected virtual TblLanguages GetLanguageFromBrowserSettings()
        {
            if (HttpContext?.Request?.UserLanguages == null)
                return null;

            // Get user preferred language from Accept-Language header (ignore English)
            var userLanguage =
                HttpContext?.Request.UserLanguages.FirstOrDefault(p =>
                    !p.StartsWith("en", StringComparison.OrdinalIgnoreCase)) ??
                HttpContext?.Request.UserLanguages.FirstOrDefault();

            if (string.IsNullOrEmpty(userLanguage) || userLanguage.Length < 2)
                return null;

            var language = _languagesService
                .GetAsEnumerable()
                .FirstOrDefault(l =>
                    userLanguage.Substring(0, 2).Trim().Equals(l.IsoCode, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published)
            {
                return language;
            }

            return null;
        }

        protected virtual TblLanguages GetLanguageFromBrowserCookies()
        {
            var userLanguage = HttpContext?.Request?.Cookies?["UserSetting"];
            if (userLanguage?.Values["Language"] == null)
                return null;

            var language = _languagesService
                .GetAsEnumerable()
                .FirstOrDefault(l => userLanguage.Values["Language"].Trim().Equals(l.IsoCode, StringComparison.InvariantCultureIgnoreCase));
            if (language != null && language.Published)
            {
                return language;
            }

            return null;
        }

        protected virtual TblCurrencies GetCurrencyFromSession()
        {
            if (HttpContext?.Session?["CurrentCurrencyISO"] == null)
                return null;

            var currencyIso = HttpContext?.Session["CurrentCurrencyISO"].ToString();
            var currency = _currencyService.FindByIso(currencyIso.Trim().ToLower());
            
            if (currency != null && currency.Published)
            {
                return currency;
            }

            return null;
        }

        protected virtual TblCurrencies GetCurrencyFromBrowserCookies()
        {
            var userCurrency = HttpContext?.Request?.Cookies?["UserSetting"];
            if (userCurrency?.Values["Currency"] == null)
                return null;

            var currency = _currencyService
                .GetAsEnumerable()
                .FirstOrDefault(l => userCurrency.Values["Currency"].Trim().Equals(l.IsoCode, StringComparison.InvariantCultureIgnoreCase));
            if (currency != null && currency.Published)
            {
                return currency;
            }

            return null;
        }

        public virtual TblLanguages CurrentLanguage
        {
            get
            {
                TblLanguages detectedLanguage = null;
                
                //get language from URL
                detectedLanguage = GetLanguageFromUrl();

                //get language from session
                if (detectedLanguage == null)
                {
                    detectedLanguage = GetLanguageFromSession();
                }

                //get language from browser cookies
                if (detectedLanguage == null)
                {
                    detectedLanguage = GetLanguageFromBrowserCookies();
                }

                //get language from browser settings
                if (detectedLanguage == null)
                {
                    detectedLanguage = GetLanguageFromBrowserSettings();
                }

                //get default language
                if (detectedLanguage == null)
                {
                    detectedLanguage = _languagesService.GetDefaultLanguage();
                }

                return detectedLanguage;
            }
        }

        public virtual TblCurrencies CurrentCurrency
        {
            get
            {
                TblCurrencies detectedcurrency = null;

                //get language from URL
                detectedcurrency = GetCurrencyFromSession();

                //get currency from browser cookies
                if (detectedcurrency == null)
                {
                    detectedcurrency = GetCurrencyFromBrowserCookies();
                }

                //get current language default currency
                if (detectedcurrency == null)
                {
                    detectedcurrency = _languagesService.FindById(CurrentLanguage.Id).DefaultCurrency;
                }

                //get default currency
                if (detectedcurrency == null)
                {
                    detectedcurrency = _currencyService.GetDefaultCurrency();
                }

                return detectedcurrency;
            }
        }

        public virtual TblUsers CurrentUser
        {
            get
            {
                if (HttpContext?.User.Identity.IsAuthenticated == true)
                {
                    var currentUserId = HttpContext.User.Identity.GetUserId();
                    return _usersService.UserManager.FindById(currentUserId);
                }

                return null;
            }
        }

        public virtual bool IsAdmin
        {
            get
            {
                if (HttpContext?.User.Identity.IsAuthenticated == true)
                    return HttpContext.User.IsInRole("Admin");

                return false;
            }
        }

        public virtual bool HasPermission(string areaName)
        {
            if (HttpContext?.User?.Identity?.IsAuthenticated == false)
            {
                return false;
            }

            if (CurrentUser?.RoleId == null)
            {
                return false;
            }

            var userRole = _userRolesService.FindById(CurrentUser.RoleId.Value);
            var permission = userRole.Permissions.FirstOrDefault(p => p.AreaName.ToLower().Trim() == areaName.ToLower().Trim());
            if (permission != null && permission.HaveAccess)
            {
                return true;
            }

            return false;
        }
    }
}
