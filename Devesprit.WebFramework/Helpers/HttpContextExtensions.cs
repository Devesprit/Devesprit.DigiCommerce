using System.Web;
using System.Web.Mvc;
using Devesprit.Core;

namespace Devesprit.WebFramework.Helpers
{
    public static partial class HttpContextExtensions
    {
        public static bool HasPermission(this HttpContextBase ctx, string areaName)
        {
            if (ctx?.User?.Identity?.IsAuthenticated == false)
            {
                return false;
            }

            return DependencyResolver.Current.GetService<IWorkContext>().HasPermission(areaName);
        }

        public static bool HasPermission(this HttpContext ctx, string areaName)
        {
            return HasPermission(new HttpContextWrapper(ctx), areaName);
        }
    }
}