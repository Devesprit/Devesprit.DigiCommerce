﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Mvc;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.DigiCommerce.Controllers.Event;
using Devesprit.Services;
using Devesprit.Services.Currency;
using Devesprit.Services.Events;
using Devesprit.Services.Languages;
using Devesprit.Services.MemoryCache;
using Devesprit.Utilities.Extensions;
using Devesprit.WebFramework;
using Devesprit.WebFramework.Helpers;
using Devesprit.WebFramework.ModelBinder;
using Elmah;
using Mapster;
using Microsoft.AspNet.Identity;

namespace Devesprit.DigiCommerce
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            if (!Directory.Exists(Server.MapPath("App_Data")))
            {
                Directory.CreateDirectory(Server.MapPath("App_Data"));
            }

            if (!Directory.Exists(Server.MapPath("Plugins")))
            {
                Directory.CreateDirectory(Server.MapPath("Plugins"));
            }

            MethodCache.GetVaryByCustom += MethodCacheGetVaryByCustom;

            ConfigAutofac();
            ConfigMapster();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleTable.EnableOptimizations = true;
            ModelBinders.Binders.Add(typeof(LocalizedString), new LocalizedStringModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorThemeViewEngine());

            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;

            //most of API providers require TLS 1.2 nowadays
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            //disable "X-AspNetMvc-Version" header name
            MvcHandler.DisableMvcResponseHeader = true;
        }

        private string MethodCacheGetVaryByCustom(string str)
        {
            if (str.Trim().ToLower() == "lang")
            {
                return Thread.CurrentThread.CurrentUICulture.Name;
            }

            if (str.Trim().ToLower() == "user")
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        return context.User.Identity.GetUserId();
                    }
                }

                return "none-unknown";
            }

            return str;
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            Response.BufferOutput = true;
            AntiForgeryConfig.RequireSsl = HttpContext.Current.Request.IsSecureConnection;
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.Url.AbsolutePath.ToLowerInvariant().StartsWith("/elmah.axd"))
            {
                if (!User.HasPermission("SiteSettings_ApplicationErrorsLog"))
                {
                    var routeData = new RouteData();
                    IController controller = new ErrorController();
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "AccessPermissionError");
                    controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
            }

            if (Request.Url.AbsolutePath.ToLowerInvariant().StartsWith("/hangfire"))
            {
                if (!User.HasPermission("ManageBackgroundJobs_BackgroundJobServer"))
                {
                    var routeData = new RouteData();
                    IController controller = new ErrorController();
                    routeData.Values.Add("controller", "Error");
                    routeData.Values.Add("action", "AccessPermissionError");
                    controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
                }
            }
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                var eventPublisher = DependencyResolver.Current.GetService<IEventPublisher>();

                //Set Current Language
                var languagesService = DependencyResolver.Current.GetService<ILanguagesService>();
                if (!string.IsNullOrWhiteSpace(Request.Form["usl"]))
                {
                    var lang = Request.Form["usl"].Trim().ToLower();
                    if (languagesService.GetAllLanguagesIsoList().Contains(lang))
                    {
                        Response.Clear();

                        HttpContext.Current.Session["CurrentLanguageISO"] = lang;

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

                        eventPublisher.Publish(new CurrentLanguageChangeEvent(lang));

                        Response.Redirect(Request.RawUrl.TrimEnd("/"));
                        Response.Flush();
                        Response.End();
                        return;
                    }
                }


                //Set Current Currency
                var currencyService = DependencyResolver.Current.GetService<ICurrencyService>();
                if (!string.IsNullOrWhiteSpace(Request.Form["usc"]))
                {
                    var currency = Request.Form["usc"].Trim().ToLower();
                    if (currencyService.GetAllCurrenciesIsoList().Contains(currency))
                    {
                        Response.Clear();

                        HttpContext.Current.Session["CurrentCurrencyISO"] = currency;

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

                        eventPublisher.Publish(new CurrentCurrencyChangeEvent(currency));

                        Response.Redirect(Request.RawUrl.TrimEnd("/"));
                        Response.Flush();
                        Response.End();
                        return;
                    }
                }
            }

            //Set Current Thread Culture
            var langIso = DependencyResolver.Current.GetService<IWorkContext>().CurrentLanguage.IsoCode;
            if (HttpContext.Current?.Request.RequestContext?.RouteData?.DataTokens["area"]?.ToString().ToLower() == "admin")
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(langIso);

                var newCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                newCulture.DateTimeFormat.Calendar = new GregorianCalendar();
                Thread.CurrentThread.CurrentCulture = newCulture;
                Thread.CurrentThread.CurrentUICulture = newCulture;
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(langIso);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(langIso);
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            // Clear the error on server.
            Server.ClearError();

            var routeData = new RouteData();
            IController controller = new ErrorController();

            var httpException = exception as HttpException;
            if (httpException == null)
            {
                var errorCode = ErrorLog.GetDefault(HttpContext.Current).Log(new Error(exception, HttpContext.Current));

                routeData.Values.Clear();
                routeData.Values.Add("controller", "Error");
                routeData.Values.Add("action", "Index");
                routeData.Values.Add("errorCode", errorCode);
                controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));
            }
            else //It's an Http Exception, Let's handle it.
            {
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // Page not found.

                        routeData.Values.Clear();
                        routeData.Values.Add("controller", "Error");
                        routeData.Values.Add("action", "PageNotFound");
                        controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));

                        break;
                    default:
                        var errorCode = ErrorLog.GetDefault(HttpContext.Current).Log(new Error(exception, HttpContext.Current));

                        routeData.Values.Clear();
                        routeData.Values.Add("controller", "Error");
                        routeData.Values.Add("action", "Index");
                        routeData.Values.Add("errorCode", errorCode);
                        controller.Execute(new RequestContext(new HttpContextWrapper(Context), routeData));

                        break;
                }
            }
        }
        
        private void ConfigAutofac()
        {
            var builder = new ContainerBuilder();
            var typeFinder = new TypeFinder();
            var assemblies = typeFinder.GetAssemblies().ToArray();

            // OPTIONAL: Register model binders that require DI.
            builder.RegisterModelBinders(assemblies);
            builder.RegisterModelBinderProvider();

            builder.RegisterAssemblyTypes(assemblies);

            // OPTIONAL: Register web abstractions like HttpContextBase.
            builder.RegisterModule<AutofacWebTypesModule>();

            // OPTIONAL: Enable property injection in view pages.
            builder.RegisterSource(new ViewRegistrationSource());

            // OPTIONAL: Enable property injection into action filters.
            builder.RegisterFilterProvider();

            // Register your MVC controllers.
            builder.RegisterControllers(assemblies).EnableClassInterceptors();

            //Register All Other Services
            //dependencies
            builder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

            //register dependencies provided by other assemblies
            var drTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
            var drInstances = new List<IDependencyRegistrar>();
            foreach (var drType in drTypes)
            {
                //Ignore not installed plugins
                var plugin = PluginManager.FindPluginByType(drType);
                if (plugin != null && !plugin.Installed)
                    continue;

                drInstances.Add((IDependencyRegistrar)Activator.CreateInstance(drType));
            }

            //sort
            drInstances = drInstances.AsQueryable().OrderBy(t => t.Order).ToList();
            foreach (var dependencyRegistrar in drInstances)
                dependencyRegistrar.Register(builder, typeFinder);
            
            var container = builder.Build();
            
            // Set the dependency resolver to be Autofac.
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private void ConfigMapster()
        {
            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
            TypeAdapterConfig.GlobalSettings.Default.ShallowCopyForSameType(true);

            TypeAdapterConfig.GlobalSettings.ForType<byte[], HttpPostedFileBase>()
                .MapWith(src => Utils.ConstructHttpPostedFile(src, ""));
            TypeAdapterConfig.GlobalSettings.ForType<HttpPostedFileBase, byte[]>()
                .MapWith(src => src.InputStream.ToByteArray());
            TypeAdapterConfig.GlobalSettings.ForType<string, HttpPostedFileBase>()
                .MapWith(src => Utils.ConstructHttpPostedFile(null, src));
            TypeAdapterConfig.GlobalSettings.ForType<string, LocalizedString>()
                .MapWith(src=> new LocalizedString(src));
            TypeAdapterConfig.GlobalSettings.ForType<LocalizedString, string>()
                .MapWith(src => src != null && src.ContainsKey(0) ? src[0] : "");
            TypeAdapterConfig.GlobalSettings.ForType<LocalizedString, LocalizedString>()
                .MapWith(src => new LocalizedString(src));

            TypeAdapterConfig.GlobalSettings.Compile();
        }
        
        public override string GetVaryByCustomString(HttpContext context, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return base.GetVaryByCustomString(context, value);

            var paramsList = value.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim().ToLower()).ToList();
            var result = string.Empty;
            if (paramsList.Contains("lang"))
            {
                result += Thread.CurrentThread.CurrentUICulture.Name + ";";
            }
            if (paramsList.Contains("user"))
            {
                if (context.User?.Identity?.IsAuthenticated == true)
                {
                    result += context.User.Identity.GetUserId() + ";";
                }
                else
                {
                    result += "none;";
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }

            return base.GetVaryByCustomString(context, value);
        }
    }
}
