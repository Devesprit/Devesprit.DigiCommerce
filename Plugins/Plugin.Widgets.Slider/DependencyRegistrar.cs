using Autofac;
using Devesprit.Core;
using Devesprit.Data;
using Devesprit.Services.AdminAreaMenu;
using Plugin.Widgets.Slider.DB;

namespace Plugin.Widgets.Slider
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<SliderAdminAreaMenu>().As<IAdminAreaPluginMenu>().InstancePerLifetimeScope();
            builder.RegisterType<SliderDbContextCustomizer>().As<IDbContextCustomizer>().InstancePerLifetimeScope();
            builder.RegisterType<SliderDbContext>().As<SliderDbContext>().InstancePerLifetimeScope();
        }

        public int Order => 1;
    }
}