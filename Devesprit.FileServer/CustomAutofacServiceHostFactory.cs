using System;
using System.ServiceModel;
using Devesprit.Utilities.Extensions;

namespace Devesprit.FileServer
{ 
    public partial class CustomAutofacServiceHostFactory : Autofac.Integration.Wcf.AutofacServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            GlobalStaticClass.HostUrl = baseAddresses[0].GetHostUrl().TrimEnd('/');
            return base.CreateServiceHost(serviceType, baseAddresses);
        }
    }

    internal static partial class GlobalStaticClass
    {
        public static string HostUrl = "";
    }
}