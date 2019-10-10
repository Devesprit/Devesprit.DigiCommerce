using System;
using System.Diagnostics;
using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Devesprit.Core;
using Devesprit.Core.Localization;
using Devesprit.Core.Plugin;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Data.Migrations;
using Devesprit.Services.AdminAreaMenu;
using Devesprit.Services.Blog;
using Devesprit.Services.Comments;
using Devesprit.Services.Countries;
using Devesprit.Services.Currency;
using Devesprit.Services.EMail;
using Devesprit.Services.Events;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.Services.FileServers;
using Devesprit.Services.Invoice;
using Devesprit.Services.Languages;
using Devesprit.Services.Localization;
using Devesprit.Services.MemoryCache;
using Devesprit.Services.NavBar;
using Devesprit.Services.Notifications;
using Devesprit.Services.Pages;
using Devesprit.Services.PaymentGateway;
using Devesprit.Services.Posts;
using Devesprit.Services.Products;
using Devesprit.Services.Redirects;
using Devesprit.Services.SearchEngine;
using Devesprit.Services.Settings;
using Devesprit.Services.SEO;
using Devesprit.Services.SocialAccounts;
using Devesprit.Services.Taxes;
using Devesprit.Services.TemplateEngine;
using Devesprit.Services.ThemeManager;
using Devesprit.Services.Users;
using Devesprit.Services.Widget;
using Devesprit.WebFramework.ResourceBundler;
using Devesprit.WebFramework.Routes;

namespace Devesprit.WebFramework
{
    public partial class DependencyRegistrar: IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.Register(c => new MethodCache());
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            builder.RegisterType<AppDbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DbInitializer>().As<IApplicationDbInitializer>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<WebHelper>().As<IWebHelper>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<WorkContext>().As<IWorkContext>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ProductService>().As<IProductService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<NavBarService>().As<INavBarService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostCategoriesService>().As<IPostCategoriesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostTagsService>().As<IPostTagsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<LanguagesService>().As<ILanguagesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<InvoiceService>().As<IInvoiceService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<PagesService>().As<IPagesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<PostAttributesService>().As<IPostAttributesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductCheckoutAttributesService>().As<IProductCheckoutAttributesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<PostAttributesMappingService>().As<IPostAttributesMappingService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<PostImagesService>().As<IPostImagesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductDiscountsForUserGroupsService>().As<IProductDiscountsForUserGroupsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<PostDescriptionsService>().As<IPostDescriptionsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductDownloadsLogService>().As<IProductDownloadsLogService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<UserLikesService>().As<IUserLikesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<UserWishlistService>().As<IUserWishlistService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<SettingService>().As<ISettingService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CountriesService>().As<ICountriesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<SocialAccountsService>().As<ISocialAccountsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<UsersService>().As<IUsersService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<UserMessagingService>().As<IUserMessagingService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<UserGroupsService>().As<IUserGroupsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<RedirectsService>().As<IRedirectsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope(); 
            builder.RegisterType<EmailService>().As<IEmailService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<TemplateEngine>().As<ITemplateEngine>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PaymentGatewayManager>().As<IPaymentGatewayManager>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<Services.LicenseManager.LicenseManager>().As<Services.LicenseManager.ILicenseManager>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<FileServersService>().As<IFileServersService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<TaxesService>().As<ITaxesService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<LuceneSearchEngine>().As<ISearchEngine>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<CommentsService>().As<ICommentsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<NotificationsService>().As<INotificationsService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<SitemapItem>().As<ISitemapItem>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<WidgetService>().As<IWidgetService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<AdminAreaMenuManager>().As<IAdminAreaMenuManager>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ResourceBundler.ResourceBundler>().As<IResourceBundler>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<ExternalLoginProviderManager>().As<IExternalLoginProviderManager>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<Services.StartupTask>().As<IStartupTask>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<BlogPostService>().As<IBlogPostService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<PostService<TblPosts>>().As<PostService<TblPosts>>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(PostService<>)).As(typeof(IPostService<>)).EnableInterfaceInterceptors().InstancePerLifetimeScope();


            //Register event consumers
            var consumers = typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList();
            foreach (var consumer in consumers)
            {
                var interfaceType = consumer.FindInterfaces((type, criteria) =>
                {
                    var isMatch = type.IsGenericType &&
                                  ((Type) criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                    return isMatch;
                }, typeof(IConsumer<>));

                builder.RegisterType(consumer).As(interfaceType).EnableInterfaceInterceptors().InstancePerLifetimeScope();
            }
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().EnableInterfaceInterceptors().InstancePerLifetimeScope();
        }

        public int Order => 0;
    }
}