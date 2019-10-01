using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services;
using Devesprit.Services.EMail;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Devesprit.DigiCommerce
{
    public partial class ApplicationUserManager : UserManager<TblUsers>
    {
        public ApplicationUserManager(IUserStore<TblUsers> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var db = context.Get<AppDbContext>();
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var emailService = DependencyResolver.Current.GetService<IEmailService>();
            var manager = new ApplicationUserManager(new UserStore<TblUsers>(db));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<TblUsers>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true,
            };
            
            manager.PasswordValidator = new CustomIdentityPasswordValidator
            {
                RequiredLength = settings.PasswordRequiredLength,
                RequireNonLetterOrDigit = settings.PasswordRequireNonLetterOrDigit,
                RequireDigit = settings.PasswordRequireDigit,
                RequireLowercase = settings.PasswordRequireLowercase,
                RequireUppercase = settings.PasswordRequireUppercase,
            };
            
            manager.UserLockoutEnabledByDefault = settings.UserLockoutEnabled;
            manager.DefaultAccountLockoutTimeSpan = settings.AccountLockoutTimeSpan;
            manager.MaxFailedAccessAttemptsBeforeLockout = settings.MaxFailedAccessAttemptsBeforeLockout;

            manager.EmailService = emailService;

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<TblUsers>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    public partial class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(new RoleStore<IdentityRole>(context.Get<AppDbContext>()));
        }
    }

    public partial class IdentityDbInitializer : IApplicationDbInitializer
    {
        //Create User=Admin@Admin.com with password=Admin@123456 in the Admin role
        public virtual void Initialize(AppDbContext db)
        {
            ApplicationUserManager userManager;
            ApplicationRoleManager roleManager;
            try
            {
                userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
                if (userManager == null || roleManager == null)
                {
                    throw new Exception();
                }
            }
            catch
            {
                userManager = new ApplicationUserManager(new UserStore<TblUsers>(db));
                roleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(db));
            }
            
            const string name = "admin@admin.com";
            const string password = "Admin@123456";
            const string roleName = "Admin";

            //Create Admin Role if it does not exist
            var role = roleManager.FindByName(roleName);
            if (role == null)
            {
                role = new IdentityRole(roleName);
                roleManager.Create(role);
                
                var user = userManager.FindByName(name);
                if (user == null)
                {
                    user = new TblUsers()
                    {
                        UserName = name,
                        Email = name,
                        RegisterDate = DateTime.Now,
                        EmailConfirmed = true,
                        FirstName = "Admin",
                        MaxDownloadCount = 0,
                    };
                    userManager.Create(user, password);
                    userManager.SetLockoutEnabled(user.Id, false);
                }

                // Add user admin to Role Admin if not already added
                var rolesForUser = userManager.GetRoles(user.Id);
                if (!rolesForUser.Contains(role.Name))
                {
                    userManager.AddToRole(user.Id, role.Name);
                }

                db.SaveChanges();
            }
        }
    }

    public partial class ApplicationSignInManager : SignInManager<TblUsers, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager,
            IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager)
        {}

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(TblUsers user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}