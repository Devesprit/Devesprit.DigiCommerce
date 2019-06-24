using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Autofac;
using Autofac.Integration.Mvc;
using Devesprit.Core;
using Devesprit.Core.Plugin;

namespace Devesprit.WebFramework.Routes
{
    public partial class RoutePublisher : IRoutePublisher
    {
        protected readonly ITypeFinder TypeFinder;

        public RoutePublisher(ITypeFinder typeFinder)
        {
            TypeFinder = typeFinder;
        }

        public virtual void RegisterRoutes(RouteCollection routes)
        {
            var routeProviderTypes = TypeFinder.FindClassesOfType<IRouteProvider>();
            var routeProviders = new List<IRouteProvider>();
            foreach (var providerType in routeProviderTypes)
            {
                //Ignore not installed plugins
                var plugin = PluginManager.FindPluginByType(providerType);
                if (plugin != null && !plugin.Installed)
                    continue;

                var constructors = providerType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    try
                    {
                        var parameters = constructor.GetParameters();
                        var parameterInstances = new List<object>();
                        foreach (var parameter in parameters)
                        {
                            var service =
                                AutofacDependencyResolver.Current.RequestLifetimeScope.Resolve(parameter.ParameterType);
                            if (service == null)
                                throw new Exception($"RoutePublisher Unknown Dependency ({providerType.FullName})");
                            parameterInstances.Add(service);
                        }
                        var provider =
                            Activator.CreateInstance(providerType, parameterInstances.ToArray()) as IRouteProvider;
                        routeProviders.Add(provider);
                        break;
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            routeProviders = routeProviders.OrderByDescending(rp => rp.Priority).ToList();
            routeProviders.ForEach(rp => rp.RegisterRoutes(routes));
        }
    }
}
