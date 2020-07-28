using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Devesprit.Utilities.Extensions
{
    public static partial class UriExtensions
    {
        public static string GetHostUrl(this Uri uri)
        {
            return uri.Scheme + Uri.SchemeDelimiter + uri.Host + (uri.Port > 0 && uri.Port != 443 && uri.Port != 80 ? ":" + uri.Port : "");
        }

        public static string GetPathAndQueryAndFragment(this Uri uri)
        {
            return uri.AbsolutePath + uri.Query + uri.Fragment;
        }

        public static Uri SetLangIso(this Uri uri, string langIso, List<string> allLanguagesIso)
        {
            var haveBackSlash = uri.ToString().EndsWith("/");
            uri = uri.RemoveLangIso(allLanguagesIso);
            var host = uri.GetHostUrl().TrimEnd("/");
            var path = uri.GetPathAndQueryAndFragment().TrimStart("/");

            if (!string.IsNullOrWhiteSpace(path))
            {
                return new Uri(host + "/" + langIso + "/" + path + (haveBackSlash ? "/" : ""));
            }
            return new Uri(host + "/" + langIso + (haveBackSlash ? "/" : ""));
        }

        public static Uri RemoveLangIso(this Uri uri, List<string> allLanguagesIso)
        {
            var haveBackSlash = uri.ToString().EndsWith("/");
            var host = uri.GetHostUrl().TrimEnd("/");
            var path = uri.GetPathAndQueryAndFragment().TrimStart("/");

            foreach (var iso in allLanguagesIso)
            {
                var isLocaleDefined = path.StartsWith(iso + "/",
                                          StringComparison.InvariantCultureIgnoreCase) ||
                                      path.StartsWith(iso + "?",
                                          StringComparison.InvariantCultureIgnoreCase) ||
                                      path.StartsWith(iso + "#",
                                          StringComparison.InvariantCultureIgnoreCase) ||
                                      path.Equals(iso,
                                          StringComparison.InvariantCultureIgnoreCase);

                if (isLocaleDefined)
                {
                    path = path.TrimStart(iso, StringComparison.InvariantCultureIgnoreCase).TrimStart("/");
                }
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                return new Uri(host + "/" + path + (haveBackSlash ? "/" : ""));
            }

            return new Uri(host + (haveBackSlash ? "/" : ""));
        }
    }
}
