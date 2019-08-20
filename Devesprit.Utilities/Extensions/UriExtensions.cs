using System;

namespace Devesprit.Utilities.Extensions
{
    public static partial class UriExtensions
    {
        public static string GetHostUrl(this Uri uri)
        {
            return uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.Port > 0 && uri.Port != 443 && uri.Port != 80 ? ":" + uri.Port : "");
        }
    }
}
