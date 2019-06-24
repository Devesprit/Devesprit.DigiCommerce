using System.Web;

namespace Devesprit.Utilities.Extensions
{
    public static partial class HttpContextExtensions
    {
        public static string GetClientIpAddress(this HttpContextBase context)
        {
            string ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }

        public static string GetClientIpAddress(this HttpContext context)
        {
            string ip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
        }
    }
}
