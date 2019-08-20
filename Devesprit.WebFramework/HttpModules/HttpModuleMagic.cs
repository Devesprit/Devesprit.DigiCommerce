using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Devesprit.WebFramework.HttpModules;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: PreApplicationStartMethod(typeof(ContainerHttpModule), "Start")]
namespace Devesprit.WebFramework.HttpModules
{
    public class ContainerHttpModule : IHttpModule
    {
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(ContainerHttpModule));
        }

        readonly Lazy<IEnumerable<IHttpModule>> _modules
            = new Lazy<IEnumerable<IHttpModule>>(RetrieveModules);

        private static IEnumerable<IHttpModule> RetrieveModules()
        {
            return DependencyResolver.Current.GetServices<IHttpModule>();
        }

        public void Dispose()
        {
            var modules = _modules.Value;
            foreach (var module in modules)
            {
                var disposableModule = module as IDisposable;
                disposableModule?.Dispose();
            }
        }

        public void Init(HttpApplication context)
        {
            var modules = _modules.Value;
            foreach (var module in modules)
            {
                module.Init(context);
            }
        }
    }
}
