using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using AutoMapper;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Controllers;
using Devesprit.DigiCommerce.Models.Post;
using Devesprit.DigiCommerce.Models.Products;
using Devesprit.Services;
using Devesprit.WebFramework;
using Devesprit.WebFramework.ModelBinder;
using Elmah;

namespace Devesprit.DigiCommerce
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
#if DEBUG
            //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
#endif

            if (!Directory.Exists(Server.MapPath("App_Data")))
            {
                Directory.CreateDirectory(Server.MapPath("App_Data"));
            }

            if (!Directory.Exists(Server.MapPath("Plugins")))
            {
                Directory.CreateDirectory(Server.MapPath("Plugins"));
            }

            ConfigAutofac();
            ConfigAutoMapper();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleTable.EnableOptimizations = true;
            ModelBinders.Binders.Add(typeof(LocalizedString), new LocalizedStringModelBinder());
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorThemeViewEngine());
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            var httpException = exception as HttpException;

            var routeData = new RouteData();
            routeData.Values.Add("controller", "Error");
            if (DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>().AppendLanguageCodeToUrl)
            {
                routeData.Values.Add("lang",
                    DependencyResolver.Current.GetService<IWorkContext>().CurrentLanguage.IsoCode);
            }

            if (httpException == null)
            {
                routeData.Values.Add("action", "Index");
                // Log the exception.
                var errorCode = ErrorLog.GetDefault(HttpContext.Current).Log(new Error(Server.GetLastError(), System.Web.HttpContext.Current));
                routeData.Values.Add("errorCode", errorCode);
            }
            else //It's an Http Exception, Let's handle it.
            {
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // Page not found.
                        routeData.Values.Add("action", "PageNotFound");
                        break;
                    default:
                        routeData.Values.Add("action", "Index");
                        // Log the exception.
                        var errorCode = ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(exception, System.Web.HttpContext.Current));
                        routeData.Values.Add("errorCode", errorCode);
                        break;
                }
            }

            Response.Clear();
            // Clear the error on server.
            Server.ClearError();
            Response.ContentType = "text/html";
            // Avoid IIS7 getting in the middle
            Response.TrySkipIisCustomErrors = true;

            // Call target Controller and pass the routeData.
            IController errorController = new ErrorController();
            errorController.Execute(new RequestContext(
                new HttpContextWrapper(HttpContext.Current), routeData));
        }
        
        private void ConfigAutofac()
        {
            var builder = new ContainerBuilder();
            var typeFinder = new TypeFinder();
            var assemblies = typeFinder.GetAssemblies().ToArray();

            // Register your MVC controllers. (MvcApplication is the name of
            // the class in Global.asax.)
            builder.RegisterControllers(assemblies);

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

        private void ConfigAutoMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.ValidateInlineMaps = false;
                cfg.CreateMissingTypeMaps = true;
                cfg.AllowNullCollections = true;
                cfg.AllowNullDestinationValues = true;

                cfg.CreateMap<TblProducts, ProductCardViewModel>();
                cfg.CreateMap<TblPosts, ProductCardViewModel>();
                cfg.CreateMap<TblPosts, PostCardViewModel>();
                cfg.CreateMap<TblBlogPosts, PostCardViewModel>();
                cfg.CreateMap<byte[], HttpPostedFileBase>().ConstructUsing(p => Utils.ConstructHttpPostedFile(p, ""));
                cfg.CreateMap<HttpPostedFileBase, byte[]>().ConstructUsing(p => p.InputStream.ToByteArray());
                cfg.CreateMap<string, HttpPostedFileBase>().ConstructUsing(p => Utils.ConstructHttpPostedFile(null, p));
                cfg.CreateMap<object, LocalizedString>().ConstructUsing(p => new LocalizedString());
                cfg.CreateMap<LocalizedString, string>().ConstructUsing(LocalizedStringToString);
                cfg.CreateMap<LocalizedString, LocalizedString>().ConstructUsing(LocalizedStringToLocalizedString);
            });
        }

        private LocalizedString LocalizedStringToLocalizedString(LocalizedString arg1, ResolutionContext arg2)
        {
            var result = new LocalizedString();
            if (arg1 != null)
            {
                foreach (var key in arg1.Keys)
                {
                    result.Add(key, arg1[key]);
                }
            }
            return result;
        }

        private string LocalizedStringToString(LocalizedString arg1, ResolutionContext arg2)
        {
            if (arg1 != null && arg1.ContainsKey(0))
            {
                return arg1[0];// default local value
            }
            return "";
        }
    }
}
