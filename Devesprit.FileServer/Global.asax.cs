using System.IO;
using Autofac;
using Autofac.Extras.CommonServiceLocator;
using Autofac.Integration.Wcf;
using CommonServiceLocator;
using Devesprit.FileServer.Repository;
using Devesprit.FileServer.Repository.Interfaces;
using Devesprit.Services.MemoryCache;


namespace Devesprit.FileServer
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            if (!Directory.Exists(Server.MapPath("App_Data")))
            {
                Directory.CreateDirectory(Server.MapPath("App_Data"));
            }

            ConfigAutofac();
        }

        private void ConfigAutofac()
        {
            var builder = new ContainerBuilder();

            // Register service implementations.
            builder.RegisterType<FileManagerService>();
            builder.RegisterType<FileManagerRepository>().As<IFileManagerRepository>().InstancePerLifetimeScope();
            builder.RegisterType<FileUploadService>();
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().InstancePerLifetimeScope();

            var container = builder.Build();
            AutofacHostFactory.Container = container;
            var csl = new AutofacServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => csl);
        }
    }
}