using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Devesprit.Data
{
    public partial class AppDbContext : IdentityDbContext<TblUsers>
    {
        public AppDbContext() : base("name=SITE.CONNSTR")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Migrations.Configuration>());
        }

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                throw newException;
            }
        }

        public override async Task<int> SaveChangesAsync()
        {
            try
            {
                return await base.SaveChangesAsync();
            }
            catch (DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                throw newException;
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            DbContextCustomizer.ApplyCustomization(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TblLocalizedStrings> LocalizedStrings { get; set; }
        public DbSet<TblLocalizedProperty> LocalizedProperty { get; set; }
        public DbSet<TblSettings> Settings { get; set; }
        public DbSet<TblSocialAccounts> SocialAccounts { get; set; }
        public DbSet<TblLanguages> Languages { get; set; }
        public DbSet<TblCurrencies> Currencies { get; set; }
        public DbSet<TblCountries> Countries { get; set; }

        public DbSet<TblUserGroups> UserGroups { get; set; }
        public DbSet<TblUserLikes> UserLikes { get; set; }
        public DbSet<TblUserWishlist> UserWishlist { get; set; }
        public DbSet<TblUserMessages> UserMessages { get; set; }
        public DbSet<TblNotifications> Notifications { get; set; }

        public DbSet<TblPosts> Posts { get; set; }
        public DbSet<TblPostImages> PostImages { get; set; }
        public DbSet<TblPostDescriptions> PostDescriptions { get; set; }
        public DbSet<TblPostCategories> PostCategories { get; set; }
        public DbSet<TblPostTags> PostTags { get; set; }
        public DbSet<TblPostAttributes> PostAttributes { get; set; }
        public DbSet<TblPostAttributeOptions> PostAttributeOptions { get; set; }
        public DbSet<TblPostAttributesMapping> PostAttributesMapping { get; set; }
        public DbSet<TblPostComments> PostComments { get; set; }

        public DbSet<TblBlogPosts> BlogPosts { get; set; }

        public DbSet<TblProducts> Products { get; set; }
        public DbSet<TblProductCheckoutAttributes> ProductCheckoutAttributes { get; set; }
        public DbSet<TblProductCheckoutAttributeOptions> ProductCheckoutAttributeOptions { get; set; }
        public DbSet<TblProductDiscountsForUserGroups> ProductDiscountsForUserGroups { get; set; }
        public DbSet<TblProductDownloadsLog> ProductDownloadsLog { get; set; }
        public DbSet<TblFileServers> FileServers { get; set; }

        public DbSet<TblInvoices> Invoices { get; set; }
        public DbSet<TblInvoiceDetails> InvoiceDetails { get; set; }
        public DbSet<TblInvoiceBillingAddress> InvoiceBillingAddress { get; set; }
        public DbSet<TblTaxes> Taxes { get; set; }


        public DbSet<TblRedirects> Redirects { get; set; }
        public DbSet<TblNavBarItems> NavBarItems { get; set; }
        public DbSet<TblPages> Pages { get; set; }


    }
}
