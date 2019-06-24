using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Devesprit.Core.Localization;
using Microsoft.AspNet.Identity;

namespace Devesprit.Core
{
    public partial class CustomIdentityPasswordValidator : PasswordValidator
    {
        public override Task<IdentityResult> ValidateAsync(string item)
        {
            using (var scope = AutofacDependencyResolver.Current.ApplicationContainer.BeginLifetimeScope())
            {
                var localization = scope.Resolve<ILocalizationService>();//AutofacDependencyResolver.Current.GetService<ILocalizationService>();

                if (item == null)
                {
                    throw new ArgumentNullException(nameof(item));
                }
                var list = new List<string>();
                if (string.IsNullOrWhiteSpace(item) || item.Length < this.RequiredLength)
                {
                    list.Add(string.Format(CultureInfo.CurrentCulture, localization.GetResource("PasswordTooShort"), new object[]
                    {
                    this.RequiredLength
                    }));
                }
                if (this.RequireNonLetterOrDigit && item.All(new Func<char, bool>(this.IsLetterOrDigit)))
                {
                    list.Add(localization.GetResource("PasswordRequireNonLetterOrDigit"));
                }
                if (this.RequireDigit && item.All((char c) => !this.IsDigit(c)))
                {
                    list.Add(localization.GetResource("PasswordRequireDigit"));
                }
                if (this.RequireLowercase && item.All((char c) => !this.IsLower(c)))
                {
                    list.Add(localization.GetResource("PasswordRequireLower"));
                }
                if (this.RequireUppercase && item.All((char c) => !this.IsUpper(c)))
                {
                    list.Add(localization.GetResource("PasswordRequireUpper"));
                }
                if (list.Count == 0)
                {
                    return Task.FromResult<IdentityResult>(IdentityResult.Success);
                }
                return Task.FromResult<IdentityResult>(IdentityResult.Failed(new string[]
                {
                string.Join(" ", list)
                }));
            }
        }
    }
}