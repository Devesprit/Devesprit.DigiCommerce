using Autofac;
using Autofac.Extras.DynamicProxy;
using Devesprit.Core;
using Devesprit.Data;
using Devesprit.DigiCommerce.Areas.Admin.Factories;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Factories;
using Devesprit.DigiCommerce.Factories.Interfaces;

namespace Devesprit.DigiCommerce
{
    public partial class DependencyRegistrar: IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<IdentityDbInitializer>().As<IApplicationDbInitializer>().InstancePerLifetimeScope();

            //Model Factories
            builder.RegisterType<PostModelFactory>().As<IPostModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ProductModelFactory>().As<IProductModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<NavBarItemModelFactory>().As<INavBarItemModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<AdminProductModelFactory>().As<IAdminProductModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostDescriptionModelFactory>().As<IPostDescriptionModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostImageModelFactory>().As<IPostImageModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostAttributeMappingModelFactory>().As<IPostAttributeMappingModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ProductCheckoutAttributeModelFactory>().As<IProductCheckoutAttributeModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ProductDiscountsForUserGroupsModelFactory>().As<IProductDiscountsForUserGroupsModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ProfileModelFactory>().As<IProfileModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<InvoiceModelFactory>().As<IInvoiceModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<SocialAccountModelFactory>().As<ISocialAccountModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<SettingsModelFactory>().As<ISettingsModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<LanguageModelFactory>().As<ILanguageModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PageModelFactory>().As<IPageModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<UserModelFactory>().As<IUserModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<UserMessagesModelFactory>().As<IUserMessagesModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<RedirectModelFactory>().As<IRedirectModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyModelFactory>().As<ICurrencyModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CountryModelFactory>().As<ICountryModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<UserRoleModelFactory>().As<IUserRoleModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<FileServerModelFactory>().As<IFileServerModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<UserGroupsModelFactory>().As<IUserGroupsModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<BackgroundJobModelFactory>().As<IBackgroundJobModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostCategoriesModelFactory>().As<IPostCategoriesModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostTagsModelFactory>().As<IPostTagsModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostAttributesModelFactory>().As<IPostAttributesModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<TaxModelFactory>().As<ITaxModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CommentModelFactory>().As<ICommentModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<WidgetModelFactory>().As<IWidgetModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<StartupTask>().As<IStartupTask>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<AdminBlogPostModelFactory>().As<IAdminBlogPostModelFactory>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
        }

        public int Order => 0;
    }
}