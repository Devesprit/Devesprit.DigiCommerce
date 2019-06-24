using Autofac;
using Devesprit.Core;
using Devesprit.Services.AdminAreaMenu;

namespace Plugin.PaymentMethod.Zarinpal
{
    public partial class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<ZarinpalMenu>().As<IAdminAreaPluginMenu>().InstancePerLifetimeScope();
        }

        public int Order => 0;
    }
}