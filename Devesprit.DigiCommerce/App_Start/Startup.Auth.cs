using System;
using System.Web.Mvc;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.ExternalLoginProvider;
using Devesprit.Services.Users;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace Devesprit.DigiCommerce
{
    public partial class Startup
    {
        public virtual AppDbContext GetDbContext()
        {
            return DependencyResolver.Current.GetService<AppDbContext>();
        }

        public virtual void ConfigureAuth(IAppBuilder app)
        {
            var usersService = DependencyResolver.Current.GetService<IUsersService>();

            // Configure the db context, user manager and role manager to use a single instance per request
            app.CreatePerOwinContext(GetDbContext);
            app.CreatePerOwinContext<UserStore<TblUsers>>((opt, cont) => new UserStore<TblUsers>(cont.Get<AppDbContext>()));
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/User/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, TblUsers>(
                        validateInterval: TimeSpan.FromMinutes(10),
                        regenerateIdentity: (manager, users) => users.GenerateUserIdentityAsync(manager))
                },
                ExpireTimeSpan = TimeSpan.FromDays(30),
                SlidingExpiration = true,
                ReturnUrlParameter = "returnUrl",
                LogoutPath = new PathString("/User/LogOff"),
                CookieSecure = CookieSecureOption.SameAsRequest,
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            DependencyResolver.Current.GetService<IExternalLoginProviderManager>().RegisterExternalLoginProviders(app);
        }
    }
}