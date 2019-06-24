using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Integration.Mvc;

namespace Devesprit.Core.Plugin
{
    public partial class PluginDescriptor : IComparable<PluginDescriptor>
    {
        public virtual string PluginFileName { get; set; }

        public virtual Type PluginType { get; set; }

        public virtual Assembly ReferencedAssembly { get; internal set; }

        public virtual FileInfo OriginalAssemblyFile { get; internal set; }

        public virtual string Group { get; set; }

        public virtual string FriendlyName { get; set; }

        public virtual string SystemName { get; set; }

        public virtual string Version { get; set; }

        public virtual string Author { get; set; }

        public virtual string Description { get; set; }

        public virtual int DisplayOrder { get; set; }

        public virtual bool Installed { get; set; }

        public virtual T Instance<T>() where T : class, IPlugin
        {
            var scope = AutofacDependencyResolver.Current.RequestLifetimeScope;
            if (!scope.TryResolve(PluginType, out var instance))
            {
                //not resolved
                var constructors = PluginType.GetConstructors();
                foreach (var constructor in constructors)
                {
                    try
                    {
                        var parameters = constructor.GetParameters();
                        var parameterInstances = new List<object>();
                        foreach (var parameter in parameters)
                        {
                            var service = scope.Resolve(parameter.ParameterType);
                            if (service == null) throw new Exception($"Plugin Unknown Dependency ({FriendlyName})");
                            parameterInstances.Add(service);
                        }
                        instance = Activator.CreateInstance(PluginType, parameterInstances.ToArray());
                        break;
                    }
                    catch 
                    {}
                }
            }

            if (instance == null)
            {
                throw new Exception($"No constructor was found that had all the dependencies satisfied ({FriendlyName})");
            }

            var typedInstance = instance as T;
            if (typedInstance != null)
                typedInstance.PluginDescriptor = this;
            return typedInstance;
        }

        public virtual IPlugin Instance()
        {
            return Instance<IPlugin>();
        }

        public virtual int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);

            return FriendlyName.CompareTo(other.FriendlyName);
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public override bool Equals(object obj)
        {
            return obj is PluginDescriptor other &&
                SystemName != null &&
                SystemName.Equals(other.SystemName);
        }

        public override int GetHashCode()
        {
            return SystemName.GetHashCode();
        }
    }
}
