using Autofac;
using Devesprit.Core;
using Devesprit.Data;
using Devesprit.Services.AdminAreaMenu;
using Plugin.DiscountCode.DB;

namespace Plugin.DiscountCode
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<DiscountCodeAdminAreaMenu>().As<IAdminAreaPluginMenu>().InstancePerLifetimeScope();
            builder.RegisterType<DiscountCodeDbContextCustomizer>().As<IDbContextCustomizer>().InstancePerLifetimeScope();
            builder.RegisterType<DiscountCodeDbContext>().As<DiscountCodeDbContext>().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}