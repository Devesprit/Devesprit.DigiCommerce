using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.Services;
using Devesprit.WebFramework.Helpers;

namespace Devesprit.WebFramework.ActionFilters
{
    public partial class UserHasPermission: ActionFilterAttribute
    {
        public string AreaName { get; }
        
        public UserHasPermission(string areaName)
        {
            AreaName = areaName;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.UserHasPermission(AreaName))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = DependencyResolver.Current.GetService<ILocalizationService>().GetResource("AccessPermissionErrorDesc") ,
                        ContentEncoding = Encoding.UTF8,
                    };
                }
                else if (filterContext.IsChildAction)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, false)
                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, true)
                    };
                }
            }

            base.OnActionExecuting(filterContext);
        }

        internal static string GetCurrentThemeAccessPermissionErrorPagePath(HttpContextBase ctx, bool withLayout)
        {
            var settings = DependencyResolver.Current.GetService<ISettingService>().LoadSetting<SiteSettings>();
            var currentTheme = settings.WebsiteTheme;
            currentTheme = string.IsNullOrWhiteSpace(currentTheme) ? "Default Theme" : currentTheme;
            var server = ctx.Server;
            if (withLayout)
            {
                var layoutPath = "~/Views/Shared/AccessPermissionError.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/AccessPermissionError.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/AccessPermissionError.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
            else
            {
                var layoutPath = "~/Views/Shared/Partials/_AccessPermissionError.cshtml";
                if (File.Exists(server.MapPath("~/Themes/{0}/Views/Shared/Partials/_AccessPermissionError.cshtml".FormatWith(currentTheme))))
                {
                    layoutPath = "~/Themes/{0}/Views/Shared/Partials/_AccessPermissionError.cshtml".FormatWith(currentTheme);
                }
                return layoutPath;
            }
        }
    }

    public partial class UserHasAllPermissions : ActionFilterAttribute
    {
        public string[] AreaName { get; }

        public UserHasAllPermissions(params string[] areaName)
        {
            AreaName = areaName;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.UserHasAllPermissions(AreaName))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = DependencyResolver.Current.GetService<ILocalizationService>().GetResource("AccessPermissionErrorDesc"),
                        ContentEncoding = Encoding.UTF8,
                    };
                }
                else if (filterContext.IsChildAction)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = UserHasPermission.GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, false)
                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = UserHasPermission.GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, true)
                    };
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public partial class UserHasAtLeastOnePermission : ActionFilterAttribute
    {
        public string[] AreaName { get; }

        public UserHasAtLeastOnePermission(params string[] areaName)
        {
            AreaName = areaName;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.UserHasAtLeastOnePermission(AreaName))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new ContentResult()
                    {
                        Content = DependencyResolver.Current.GetService<ILocalizationService>().GetResource("AccessPermissionErrorDesc"),
                        ContentEncoding = Encoding.UTF8,
                    };
                }
                else if (filterContext.IsChildAction)
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = UserHasPermission.GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, false)
                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = UserHasPermission.GetCurrentThemeAccessPermissionErrorPagePath(filterContext.HttpContext, true)
                    };
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}