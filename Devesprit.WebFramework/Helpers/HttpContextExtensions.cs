using System.IO;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Devesprit.Core;
using Devesprit.Core.Settings;
using Devesprit.Services;

namespace Devesprit.WebFramework.Helpers
{
    public static partial class HttpContextExtensions
    {
        public static bool UserHasPermission(this HttpContextBase ctx, string areaName)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasPermission(areaName);
        }

        public static bool UserHasPermission(this HttpContext ctx, string areaName)
        {
            return UserHasPermission(new HttpContextWrapper(ctx), areaName);
        }

        public static bool UserHasAllPermissions(this HttpContextBase ctx, params string[] areaNames)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasAllPermissions(areaNames);
        }

        public static bool UserHasAllPermissions(this HttpContext ctx, params string[] areaNames)
        {
            return UserHasAllPermissions(new HttpContextWrapper(ctx), areaNames);
        }

        public static bool UserHasAtLeastOnePermission(this HttpContextBase ctx, params string[] areaNames)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasAtLeastOnePermission(areaNames);
        }

        public static bool UserHasAtLeastOnePermission(this HttpContext ctx, params string[] areaNames)
        {
            return UserHasAtLeastOnePermission(new HttpContextWrapper(ctx), areaNames);
        }

        public static string GetCurrentThemeLayoutAddress(this HttpContextBase ctx, bool baseLayout = false)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var currentTheme = settings.WebsiteTheme;
            currentTheme = string.IsNullOrWhiteSpace(currentTheme) ? "Default Theme" : currentTheme;
            var server = ctx.Server;
            if (baseLayout)
            {
                var layoutPath = "~/Views/Shared/_BaseLayout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
            else
            {
                var layoutPath = "~/Views/Shared/_Layout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
        }

        public static string GetCurrentThemeAdminAreaLayoutAddress(this HttpContextBase ctx, bool baseLayout = false)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var currentTheme = settings.WebsiteTheme;
            currentTheme = string.IsNullOrWhiteSpace(currentTheme) ? "Default Theme" : currentTheme;
            var server = ctx.Server;
            if (baseLayout)
            {
                var layoutPath = "~/Areas/Admin/Views/Shared/_BaseLayout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Areas/Admin/Views/Shared/_BaseLayout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
            else
            {
                var layoutPath = "~/Areas/Admin/Views/Shared/_Layout.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/_Layout.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Areas/Admin/Views/Shared/_Layout.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
        }

        public static string GetCurrentThemeLayoutAddress(this HttpContext ctx, bool baseLayout = false)
        {
            return GetCurrentThemeLayoutAddress(new HttpContextWrapper(ctx), baseLayout);
        }

        public static string GetCurrentThemeAdminAreaLayoutAddress(this HttpContext ctx, bool baseLayout = false)
        {
            return GetCurrentThemeAdminAreaLayoutAddress(new HttpContextWrapper(ctx), baseLayout);
        }

        public static bool HasPermission(this IPrincipal user, string areaName)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasPermission(areaName);
        }

        public static bool HasAllPermissions(this IPrincipal user, params string[] areaNames)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasAllPermissions(areaNames);
        }
        
        public static bool HasAtLeastOnePermission(this IPrincipal user, params string[] areaNames)
        {
            return DependencyResolver.Current.GetService<IWorkContext>().UserHasAtLeastOnePermission(areaNames);
        }
    }
}