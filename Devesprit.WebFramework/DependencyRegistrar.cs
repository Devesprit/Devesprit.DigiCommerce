using System;
using System.Linq;
using Autofac;
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
            builder.RegisterType<AppDbContext>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<DbInitializer>().As<IApplicationDbInitializer>().InstancePerLifetimeScope();
            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().InstancePerLifetimeScope();
            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();
            builder.RegisterType<WorkContext>().As<IWorkContext>().InstancePerLifetimeScope();
            builder.RegisterType<NavBarService>().As<INavBarService>().InstancePerLifetimeScope();
            builder.RegisterType<PostCategoriesService>().As<IPostCategoriesService>().InstancePerLifetimeScope();
            builder.RegisterType<PostTagsService>().As<IPostTagsService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<LanguagesService>().As<ILanguagesService>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerLifetimeScope(); 
            builder.RegisterType<InvoiceService>().As<IInvoiceService>().InstancePerLifetimeScope(); 
            builder.RegisterType<PagesService>().As<IPagesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<PostAttributesService>().As<IPostAttributesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductCheckoutAttributesService>().As<IProductCheckoutAttributesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<PostAttributesMappingService>().As<IPostAttributesMappingService>().InstancePerLifetimeScope(); 
            builder.RegisterType<PostImagesService>().As<IPostImagesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductDiscountsForUserGroupsService>().As<IProductDiscountsForUserGroupsService>().InstancePerLifetimeScope(); 
            builder.RegisterType<PostDescriptionsService>().As<IPostDescriptionsService>().InstancePerLifetimeScope(); 
            builder.RegisterType<ProductDownloadsLogService>().As<IProductDownloadsLogService>().InstancePerLifetimeScope(); 
            builder.RegisterType<UserLikesService>().As<IUserLikesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<UserWishlistService>().As<IUserWishlistService>().InstancePerLifetimeScope(); 
            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizedEntityService>().As<ILocalizedEntityService>().InstancePerLifetimeScope();
            builder.RegisterType<LocalizationService>().As<ILocalizationService>().InstancePerLifetimeScope();
            builder.RegisterType<CountriesService>().As<ICountriesService>().InstancePerLifetimeScope(); 
            builder.RegisterType<SocialAccountsService>().As<ISocialAccountsService>().InstancePerLifetimeScope(); 
            builder.RegisterType<UsersService>().As<IUsersService>().InstancePerLifetimeScope(); 
            builder.RegisterType<UserMessagingService>().As<IUserMessagingService>().InstancePerLifetimeScope(); 
            builder.RegisterType<UserGroupsService>().As<IUserGroupsService>().InstancePerLifetimeScope(); 
            builder.RegisterType<RedirectsService>().As<IRedirectsService>().InstancePerLifetimeScope(); 
            builder.RegisterType<EmailService>().As<IEmailService>().InstancePerLifetimeScope();
            builder.RegisterType<TemplateEngine>().As<ITemplateEngine>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentGatewayManager>().As<IPaymentGatewayManager>().InstancePerLifetimeScope();
            builder.RegisterType<Services.LicenseManager.LicenseManager>().As<Services.LicenseManager.ILicenseManager>().InstancePerLifetimeScope();
            builder.RegisterType<FileServersService>().As<IFileServersService>().InstancePerLifetimeScope();
            builder.RegisterType<TaxesService>().As<ITaxesService>().InstancePerLifetimeScope();
            builder.RegisterType<LuceneSearchEngine>().As<ISearchEngine>().InstancePerLifetimeScope();
            builder.RegisterType<CommentsService>().As<ICommentsService>().InstancePerLifetimeScope();
            builder.RegisterType<NotificationsService>().As<INotificationsService>().InstancePerLifetimeScope();
            builder.RegisterType<SitemapGenerator>().As<ISitemapGenerator>().InstancePerLifetimeScope();
            builder.RegisterType<SitemapItem>().As<ISitemapItem>().InstancePerLifetimeScope();
            builder.RegisterType<ThemeProvider>().As<IThemeProvider>().InstancePerLifetimeScope();
            builder.RegisterType<PluginFinder>().As<IPluginFinder>().InstancePerLifetimeScope();
            builder.RegisterType<WidgetService>().As<IWidgetService>().InstancePerLifetimeScope();
            builder.RegisterType<AdminAreaMenuManager>().As<IAdminAreaMenuManager>().InstancePerLifetimeScope();
            builder.RegisterType<ResourceBundler.ResourceBundler>().As<IResourceBundler>().InstancePerLifetimeScope();
            builder.RegisterType<ExternalLoginProviderManager>().As<IExternalLoginProviderManager>().InstancePerLifetimeScope();
            builder.RegisterType<Services.StartupTask>().As<IStartupTask>().InstancePerLifetimeScope();
            builder.RegisterType<BlogPostService>().As<IBlogPostService>().InstancePerLifetimeScope();
            builder.RegisterType<PostService<TblPosts>>().As<PostService<TblPosts>>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(PostService<>)).As(typeof(IPostService<>)).InstancePerLifetimeScope();
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();


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

                builder.RegisterType(consumer).As(interfaceType).InstancePerLifetimeScope();
            }
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().InstancePerLifetimeScope();
            builder.RegisterType<SubscriptionService>().As<ISubscriptionService>().InstancePerLifetimeScope();
        }

        public int Order => 0;
    }
}