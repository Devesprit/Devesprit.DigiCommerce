using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Controllers.Event;
using Devesprit.Services;
using Devesprit.Services.Currency;
using Devesprit.Services.Events;
using Devesprit.Services.Invoice;
using Devesprit.Services.Languages;
using Devesprit.Services.Notifications;
using Devesprit.Services.Users;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework;
using Elmah;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace Devesprit.DigiCommerce.Controllers
{
    public abstract class BaseController : Controller
    {
        private IWorkContext _workContext;
        private SiteSettings _settings;
        private ILanguagesService _languagesService;
        private ICurrencyService _currencyService;
        private IEventPublisher _eventPublisher;

        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().Get<ApplicationUserManager>();
        public ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        public IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        public ILanguagesService LanguagesService => _languagesService ?? (_languagesService =
                                                         DependencyResolver.Current.GetService<ILanguagesService>()); 
        public ICurrencyService CurrencyService => _currencyService ?? (_currencyService =
                                                         DependencyResolver.Current.GetService<ICurrencyService>());
        public IWorkContext WorkContext => _workContext ?? (_workContext =
                                                       DependencyResolver.Current.GetService<IWorkContext>());
        public SiteSettings CurrentSettings => _settings ?? (_settings =
                                                       DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>());
        public IEventPublisher EventPublisher => _eventPublisher ?? (_eventPublisher =
                                                       DependencyResolver.Current.GetService<IEventPublisher>());

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Response != null)
            {
                try
                {
                    Response.AddHeader("Set-Cookie", "HttpOnly;Secure;SameSite=Lax");
                }
                catch
                { }
            }

            if (!filterContext.IsChildAction)
            {
                //Save user latest IP address
                if (User.Identity.IsAuthenticated)
                {
                    DependencyResolver.Current.GetService<IUsersService>()
                        .SetUserLatestIpAndLoginDate(User.Identity.GetUserId(), HttpContext.GetClientIpAddress());
                }

                //If user disabled
                var accountDisabledUrl = Url.Action("AccountDisabled", "User");
                if (User.Identity.IsAuthenticated && filterContext.HttpContext?.Request.Url?.LocalPath != accountDisabledUrl)
                {
                    var user = UserManager.FindById(User.Identity.GetUserId());
                    if (user != null && user.UserDisabled)
                    {
                        filterContext.Result = new RedirectResult(accountDisabledUrl, false);
                    }
                }

                //Append 'https', 'www' and current Language ISO to Url and redirect it
                if (Request.Url != null && Request.HttpMethod.ToLower() == "get")
                {
                    var mustRedirect = false;
                    var redirectToUrl = "";
                    var hostUrl = "";
                    var pathAndQuery = Request.Url.AbsolutePath + Request.Url.Query + Request.Url.Fragment;
                    var port = "";
                    if (Request.Url.Port != 443 && Request.Url.Port != 80 && Request.Url.Port != 0)
                    {
                        port = ":" + Request.Url.Port;
                    }

                    if (CurrentSettings.SiteUrl.IsValidUrl() && CurrentSettings.RedirectAllRequestsToSiteUrl)
                    {
                        hostUrl = CurrentSettings.SiteUrl.TrimEnd("/");
                    }
                    else
                    {
                        hostUrl = Request.Url.Scheme + Uri.SchemeDelimiter + Request.Url.Host + port;
                    }

                    if (CurrentSettings.AppendLanguageCodeToUrl)
                    {
                        var currentLanguage = WorkContext.CurrentLanguage;

                        var isLocaleDefined = pathAndQuery.TrimStart('/').StartsWith(currentLanguage.IsoCode + "/",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              pathAndQuery.TrimStart('/').StartsWith(currentLanguage.IsoCode + "?",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              pathAndQuery.TrimStart('/').StartsWith(currentLanguage.IsoCode + "#",
                                                  StringComparison.InvariantCultureIgnoreCase) ||
                                              pathAndQuery.TrimStart('/').Equals(currentLanguage.IsoCode,
                                                  StringComparison.InvariantCultureIgnoreCase);

                        if (!isLocaleDefined)
                        {
                            pathAndQuery = $"/{currentLanguage.IsoCode.ToLower()}{pathAndQuery}";
                            mustRedirect = true;
                        }
                    }

                    redirectToUrl = $"{hostUrl}{pathAndQuery}";

                    if (!redirectToUrl.ToLower().Trim().StartsWith($"{Request.Url.Scheme}{Uri.SchemeDelimiter}{Request.Url.Host}".ToLower().Trim().TrimEnd('/')))
                    {
                        mustRedirect = true;
                    }

                    if (mustRedirect)
                    {
                        Response.RedirectPermanent(redirectToUrl, true);
                        return;
                    }
                }


                //Set Current Language
                if (!string.IsNullOrWhiteSpace(Request.Form["usl"]))
                {
                    var lang = Request.Form["usl"].Trim().ToLower();
                    if (LanguagesService.GetAllLanguagesIsoList().Contains(lang))
                    {
                        Session["CurrentLanguageISO"] = lang;

                        var responseCookie = Request.Cookies["UserSetting"] ?? new HttpCookie("UserSetting");
                        responseCookie.Values["Language"] = lang;
                        responseCookie.Domain = "." + Request.Url.Host.TrimStart("www.").Trim();
                        responseCookie.Expires = DateTime.Now.AddYears(1).ToUniversalTime();
                        if (Request.Cookies["UserSetting"] == null)
                        {
                            Response.Cookies.Add(responseCookie);
                        }
                        else
                        {
                            Response.Cookies.Set(responseCookie);
                        }

                        EventPublisher.Publish(new CurrentLanguageChangeEvent(lang));

                        Response.Redirect(Request.RawUrl);
                        return;
                    }
                }


                //Set Current Currency
                if (!string.IsNullOrWhiteSpace(Request.Form["usc"]))
                {
                    var currency = Request.Form["usc"].Trim().ToLower();
                    if (CurrencyService.GetAllCurrenciesIsoList().Contains(currency))
                    {
                        Session["CurrentCurrencyISO"] = currency;

                        var responseCookie = Request.Cookies["UserSetting"] ?? new HttpCookie("UserSetting");
                        responseCookie.Values["Currency"] = currency;
                        responseCookie.Expires = DateTime.Now.AddYears(1).ToUniversalTime();
                        responseCookie.Domain = "." + Request.Url.Host.TrimStart("www.").Trim();
                        if (Request.Cookies["UserSetting"] == null)
                        {
                            Response.Cookies.Add(responseCookie);
                        }
                        else
                        {
                            Response.Cookies.Set(responseCookie);
                        }

                        EventPublisher.Publish(new CurrentCurrencyChangeEvent(currency));

                        Response.Redirect(Request.RawUrl);
                        return;
                    }
                }


                //Show User UnReaded Notifications Alert
                if (Session != null && (User.Identity.IsAuthenticated && Session["NotificationsAlert"]?.ToString() != User.Identity.GetUserId() && !Request.IsAjaxRequest()))
                {
                    Session["NotificationsAlert"] = User.Identity.GetUserId();
                    var notificationsService = DependencyResolver.Current.GetService<INotificationsService>();
                    var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
                    var notificationsCount = notificationsService.GetUserUnReadedNotificationsCount(User.Identity.GetUserId());
                    if (notificationsCount > 0)
                    {
                        var msg = string.Format(localizationService.GetResource("YouHaveUnreadNotifications"),
                            notificationsCount, Url.Action("Index", "Profile"));
                        AddNotification(NotificationType.Warning, msg, true);
                    }
                }
            }

            //Send current invoice items count to view
            var invoice = DependencyResolver.Current.GetService<IInvoiceService>().GetUserCurrentInvoice(false);
            ViewData["InvoiceItemsCount"] = invoice?.InvoiceDetails?.Count ?? 0;

            base.OnActionExecuting(filterContext);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;
            var errCode = ErrorLog.GetDefault(System.Web.HttpContext.Current)
                .Log(new Error(filterContext.Exception, System.Web.HttpContext.Current));
            filterContext.Result = RedirectToAction("Index", "Error", new {errorCode = errCode, area = ""});
        }

        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotificationType.Success, message, persistForTheNextRequest);
        }

        protected virtual void ErrorNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotificationType.Error, message, persistForTheNextRequest);
        }

        protected virtual void ErrorNotification(Exception exception, bool persistForTheNextRequest = true, bool logException = true)
        {
            if (logException)
                Elmah.ErrorLog.GetDefault(System.Web.HttpContext.Current)
                    .Log(new Error(exception, System.Web.HttpContext.Current));
            AddNotification(NotificationType.Error, exception.Message, persistForTheNextRequest);
        }

        protected virtual void WarningNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotificationType.Warning, message, persistForTheNextRequest);
        }

        protected virtual void AddNotification(NotificationType type, string message, bool persistForTheNextRequest)
        {
            var dataKey = $"notifications.{type}";
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }
    }
}