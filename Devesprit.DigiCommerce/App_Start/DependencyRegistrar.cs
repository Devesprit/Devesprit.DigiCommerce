using Autofac;
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
            builder.RegisterType<PostModelFactory>().As<IPostModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductModelFactory>().As<IProductModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<NavBarItemModelFactory>().As<INavBarItemModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<AdminProductModelFactory>().As<IAdminProductModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostDescriptionModelFactory>().As<IPostDescriptionModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostImageModelFactory>().As<IPostImageModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostAttributeMappingModelFactory>().As<IPostAttributeMappingModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductCheckoutAttributeModelFactory>().As<IProductCheckoutAttributeModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProductDiscountsForUserGroupsModelFactory>().As<IProductDiscountsForUserGroupsModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<ProfileModelFactory>().As<IProfileModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<InvoiceModelFactory>().As<IInvoiceModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SocialAccountModelFactory>().As<ISocialAccountModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<SettingsModelFactory>().As<ISettingsModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<LanguageModelFactory>().As<ILanguageModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PageModelFactory>().As<IPageModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UserModelFactory>().As<IUserModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UserMessagesModelFactory>().As<IUserMessagesModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<RedirectModelFactory>().As<IRedirectModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyModelFactory>().As<ICurrencyModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CountryModelFactory>().As<ICountryModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<FileServerModelFactory>().As<IFileServerModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<UserGroupsModelFactory>().As<IUserGroupsModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<BackgroundJobModelFactory>().As<IBackgroundJobModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostCategoriesModelFactory>().As<IPostCategoriesModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostTagsModelFactory>().As<IPostTagsModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PostAttributesModelFactory>().As<IPostAttributesModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<TaxModelFactory>().As<ITaxModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<CommentModelFactory>().As<ICommentModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<WidgetModelFactory>().As<IWidgetModelFactory>().InstancePerLifetimeScope();
            builder.RegisterType<StartupTask>().As<IStartupTask>().InstancePerLifetimeScope();
            builder.RegisterType<AdminBlogPostModelFactory>().As<IAdminBlogPostModelFactory>().InstancePerLifetimeScope();
        }

        public int Order => 0;
    }
}