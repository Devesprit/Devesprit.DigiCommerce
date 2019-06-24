using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Devesprit.FileServer.ElmahConfig
{
    public partial class ServiceErrorBehaviorAttribute : Attribute, IServiceBehavior
    {
        private readonly Type _errorHandlerType;
        public ServiceErrorBehaviorAttribute(Type errorHandlerType)
        {
            this._errorHandlerType = errorHandlerType;
        }

        public virtual void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public virtual void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (Activator.CreateInstance(_errorHandlerType) is IErrorHandler errorHandler)
            {
                foreach (ChannelDispatcherBase dispatcher in serviceHostBase.ChannelDispatchers)
                {
                    ChannelDispatcher cd = dispatcher as ChannelDispatcher;
                    cd.ErrorHandlers.Add(errorHandler);
                }
            }
        }

        public virtual void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}