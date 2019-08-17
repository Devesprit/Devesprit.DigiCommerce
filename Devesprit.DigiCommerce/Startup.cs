using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Transactions;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using Autofac.Integration.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Services;
using Hangfire;
using Hangfire.Dashboard.RecurringJobExtensions;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Hangfire.MySql;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using WebMarkupMin.AspNet.Brotli;
using WebMarkupMin.AspNet.Common.Compressors;
using WebMarkupMin.AspNet.Common.UrlMatchers;
using WebMarkupMin.AspNet4.Common;
using WebMarkupMin.Core;
using WebMarkupMin.Yui;
using Startup = Devesprit.DigiCommerce.Startup;

[assembly: OwinStartup(typeof(Startup))]
namespace Devesprit.DigiCommerce
{
    public partial class Startup: IRegisteredObject
    {
        public Startup()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));
            HostingEnvironment.UnregisterObject(this);
        }

        public virtual void Configuration(IAppBuilder app)
        {
            //Register DbContext Customizers
            RegisterDbContextCustomizers();

            //First connection to database (Create DB if not exist)
            DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();

            AntiForgeryConfig.SuppressXFrameOptionsHeader = true;

            ConfigureAuth(app);

            //Hangfire Background Jobs
            app.UseHangfireAspNet(GetHangfireConfiguration);
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                AppPath = VirtualPathUtility.ToAbsolute("~")
            });

            //Config Html Optimizer
            WebMarkupMinConfigure(WebMarkupMinConfiguration.Instance);

            RunStartupTasks();
        }

        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            //Config Hangfire
            LogProvider.SetCurrentLogProvider(new ElmahLogProvider(LogLevel.Error));
            var connectionString = ConfigurationManager.ConnectionStrings["HANGFIRE.CONNSTR"];
            var providerName = connectionString.ProviderName;
            if (providerName.ToLower().Contains("MySql".ToLower()))
            {
                GlobalConfiguration.Configuration
                .UseStorage(new MySqlStorage(connectionString.ConnectionString, new MySqlStorageOptions()
                {
                    PrepareSchemaIfNecessary = true,
                    TablesPrefix = "Hangfire_",
                    TransactionIsolationLevel = IsolationLevel.ReadCommitted,
                    DashboardJobListLimit = 50000,
                    TransactionTimeout = TimeSpan.FromMinutes(1),
                }))
                .UseDashboardRecurringJobExtensions()
                .UseElmahLogProvider(LogLevel.Error)
                .UseAutofacActivator(AutofacDependencyResolver.Current.ApplicationContainer)
                .UseFilter(new CanBePausedAttribute());
            }
            else
            {
                GlobalConfiguration.Configuration
                   .UseSqlServerStorage("HANGFIRE.CONNSTR", new SqlServerStorageOptions()
                   {
                       PrepareSchemaIfNecessary = true,
                       UseRecommendedIsolationLevel = true,
                       DashboardJobListLimit = 50000,
                       TransactionTimeout = TimeSpan.FromMinutes(1),
                   })
                   .UseDashboardRecurringJobExtensions()
                   .UseElmahLogProvider(LogLevel.Error)
                   .UseAutofacActivator(AutofacDependencyResolver.Current.ApplicationContainer)
                   .UseFilter(new CanBePausedAttribute());
            }

            yield return new BackgroundJobServer(
                new BackgroundJobServerOptions
                {
                    ServerName = $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}:{AppDomain.CurrentDomain.Id}",
                    WorkerCount = Environment.ProcessorCount * 5
                });
        }

        protected virtual void RegisterDbContextCustomizers()
        {
            var dbContextCustomizers = DependencyResolver.Current.GetServices<IDbContextCustomizer>().ToList();

            foreach (var dbContextCustomizer in dbContextCustomizers)
            {
                DbContextCustomizer.RegisterModelCustomization(dbContextCustomizer.OnModelCreating, dbContextCustomizer.Order);
            }
        }

        protected virtual void RunStartupTasks()
        {
            var startUpTasks = DependencyResolver.Current.GetServices<IStartupTask>().ToList();

            //sort
            startUpTasks = startUpTasks.AsQueryable().OrderBy(st => st.Order).ToList();
            foreach (var startUpTask in startUpTasks)
                startUpTask.Execute();

            foreach (var task in startUpTasks)
            {
                var disposableModule = task as IDisposable;
                disposableModule?.Dispose();
            }
        }

        public static void WebMarkupMinConfigure(WebMarkupMinConfiguration configuration)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();

            configuration.AllowMinificationInDebugMode = true;
            configuration.AllowCompressionInDebugMode = true;
            configuration.DisablePoweredByHttpHeaders = true;
            configuration.DisableCompression = !settings.EnableResponseCompression;
            configuration.DisableMinification = !settings.EnableHtmlMinification;

            var htmlMinificationManager = HtmlMinificationManager.Current;
            htmlMinificationManager.ExcludedPages = new List<IUrlMatcher>
            {
                new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
                new WildcardUrlMatcher("/Purchase/PurchaseProductWizard*"),
                new WildcardUrlMatcher("/Admin/*"),
            };
            var htmlMinificationSettings = htmlMinificationManager.MinificationSettings;
            htmlMinificationSettings.RemoveRedundantAttributes = false;
            htmlMinificationSettings.RemoveHttpProtocolFromAttributes = false;
            htmlMinificationSettings.RemoveHttpsProtocolFromAttributes = false;
            htmlMinificationSettings.WhitespaceMinificationMode = WhitespaceMinificationMode.Safe;
            htmlMinificationSettings.RemoveHtmlComments = true;
            htmlMinificationSettings.MinifyEmbeddedCssCode = true;
            htmlMinificationSettings.MinifyEmbeddedJsCode = true;
            htmlMinificationSettings.MinifyInlineCssCode = true;
            htmlMinificationSettings.MinifyInlineJsCode = true;
            htmlMinificationSettings.MinifyEmbeddedJsonData = true;
            htmlMinificationSettings.RemoveEmptyAttributes = false;
            htmlMinificationSettings.RemoveHtmlCommentsFromScriptsAndStyles = true;
            htmlMinificationSettings.RemoveOptionalEndTags = true;
            htmlMinificationManager.CssMinifierFactory = new YuiCssMinifierFactory(new YuiCssMinificationSettings()
            {
                RemoveComments = true
            });
            htmlMinificationManager.JsMinifierFactory = new YuiJsMinifierFactory();

            var xhtmlMinificationManager = XhtmlMinificationManager.Current;
            xhtmlMinificationManager.IncludedPages = new List<IUrlMatcher>
            {
                new WildcardUrlMatcher("/minifiers/x*ml-minifier"),
            };
            var xhtmlMinificationSettings = xhtmlMinificationManager.MinificationSettings;
            xhtmlMinificationSettings.RemoveRedundantAttributes = false;
            xhtmlMinificationSettings.RemoveHttpProtocolFromAttributes = false;
            xhtmlMinificationSettings.RemoveHttpsProtocolFromAttributes = false;
            xhtmlMinificationSettings.WhitespaceMinificationMode = WhitespaceMinificationMode.Safe;
            xhtmlMinificationSettings.RemoveHtmlComments = true;
            xhtmlMinificationSettings.MinifyEmbeddedCssCode = true;
            xhtmlMinificationSettings.MinifyEmbeddedJsCode = true;
            xhtmlMinificationSettings.MinifyInlineCssCode = true;
            xhtmlMinificationSettings.MinifyInlineJsCode = true;
            xhtmlMinificationSettings.MinifyEmbeddedJsonData = true;
            xhtmlMinificationSettings.RemoveEmptyAttributes = false;
            xhtmlMinificationSettings.RemoveHtmlCommentsFromScriptsAndStyles = true;
            xhtmlMinificationManager.CssMinifierFactory = new YuiCssMinifierFactory(new YuiCssMinificationSettings()
            {
                RemoveComments = true
            });
            xhtmlMinificationManager.JsMinifierFactory = new YuiJsMinifierFactory();

            var xmlMinificationManager = XmlMinificationManager.Current;
            var xmlMinificationSettings = xmlMinificationManager.MinificationSettings;
            xmlMinificationSettings.CollapseTagsWithoutContent = true;
            xmlMinificationSettings.MinifyWhitespace = true;
            xmlMinificationSettings.RemoveXmlComments = true;

            var httpCompressionManager = HttpCompressionManager.Current;
            httpCompressionManager.CompressorFactories = new List<ICompressorFactory>
            {
                new BrotliCompressorFactory(),
                new GZipCompressorFactory(),
                new DeflateCompressorFactory(),
            };
        }
    }
}