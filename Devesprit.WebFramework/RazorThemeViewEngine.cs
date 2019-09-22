using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Devesprit.Core.Settings;
using Devesprit.Services;

namespace Devesprit.WebFramework
{
    public partial class RazorThemeViewEngine : RazorViewEngine
    {
        private static readonly string[] EmptyLocations;

        private static ISettingService _settingsService;

        private static SiteSettings CurrentSettings
        {
            get
            {
                if (_settingsService == null)
                {
                    _settingsService = DependencyResolver.Current.GetService<ISettingService>();
                }

                try
                {
                    return _settingsService.LoadSetting<SiteSettings>();
                }
                catch
                {
                    _settingsService = DependencyResolver.Current.GetService<ISettingService>();
                    return _settingsService.LoadSetting<SiteSettings>();
                }
            }
        }

        public RazorThemeViewEngine()
        {
            this.AreaViewLocationFormats = new string[4]
            {
                "~/Themes/{3}/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{3}/Areas/{2}/Views/Shared/{0}.cshtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };
            this.AreaMasterLocationFormats = new string[4]
            {
                "~/Themes/{3}/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{3}/Areas/{2}/Views/Shared/{0}.cshtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };
            this.AreaPartialViewLocationFormats = new string[4]
            {
                "~/Themes/{3}/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{3}/Areas/{2}/Views/Shared/{0}.cshtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml",
            };
            this.ViewLocationFormats = new string[4]
            {
                "~/Themes/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
            };
            this.MasterLocationFormats = new string[4]
            {
                "~/Themes/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
            };
            this.PartialViewLocationFormats = new string[4]
            {
                "~/Themes/{2}/Views/{1}/{0}.cshtml",
                "~/Themes/{2}/Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
            };
            this.FileExtensions = new string[1]
            {
                "cshtml"
            };
        }

        protected static string GetAreaName(RouteData routeData)
        {
            object obj;
            if (routeData.DataTokens.TryGetValue("area", out obj))
            {
                return obj as string;
            }
            return GetAreaName(routeData.Route);
        }

        protected static string GetAreaName(RouteBase route)
        {
            IRouteWithArea routeWithArea = route as IRouteWithArea;
            if (routeWithArea != null)
            {
                return routeWithArea.Area;
            }
            Route route2 = route as Route;
            if (route2 != null && route2.DataTokens != null)
            {
                return route2.DataTokens["area"] as string;
            }
            return null;
        }

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            try
            {
                return File.Exists(controllerContext.HttpContext.Server.MapPath(virtualPath));
            }
            catch (HttpException exception)
            {
                if (exception.GetHttpCode() != 0x194)
                {
                    throw;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            string[] strArray;
            string[] strArray2;

            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("viewName must be specified.", nameof(viewName));
            }

            var themeName = GetThemeToUse(controllerContext);

            var requiredString = controllerContext.RouteData.GetRequiredString("controller");

            var viewPath = GetPath(controllerContext, ViewLocationFormats, AreaViewLocationFormats, "ViewLocationFormats", viewName, themeName,
                requiredString, "View", useCache, out strArray);
            var masterPath = GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName,
                themeName, requiredString, "Master", useCache, out strArray2);
            

            if (!string.IsNullOrEmpty(viewPath) && (!string.IsNullOrEmpty(masterPath) || string.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
            }

            return strArray2 == null ? new ViewEngineResult(strArray) : new ViewEngineResult(strArray.Union(strArray2));
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            string[] strArray;
            if (controllerContext == null)
            {
                throw new ArgumentNullException(nameof(controllerContext));
            }
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("partialViewName must be specified.", nameof(partialViewName));
            }

            var themeName = GetThemeToUse(controllerContext);

            var requiredString = controllerContext.RouteData.GetRequiredString("controller");

            var partialViewPath = GetPath(controllerContext, PartialViewLocationFormats, AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, themeName, requiredString, "Partial", useCache, out strArray);
            return string.IsNullOrEmpty(partialViewPath) ? new ViewEngineResult(strArray) : new ViewEngineResult(CreatePartialView(controllerContext, partialViewPath), this);
        }

        protected static string GetThemeToUse(ControllerContext controllerContext)
        {
            var themeName = CurrentSettings.WebsiteTheme;
            if (string.IsNullOrWhiteSpace(themeName))
            {
                themeName = "Default Theme";
            }
            return themeName;
        }

        protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string themeName, string controllerName, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
        {
            searchedLocations = EmptyLocations;
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            string areaName = GetAreaName(controllerContext.RouteData);
            List<ViewLocation> viewLocations = GetViewLocations(locations, !string.IsNullOrEmpty(areaName) ? areaLocations : null);
            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The property '{0}' cannot be null or empty.", new object[]
                {
                    locationsPropertyName
                }));
            }

            bool flag = IsSpecificPath(name);
            string key = CreateCacheKey(cacheKeyPrefix, name, flag ? string.Empty : controllerName, areaName, themeName);
            if (useCache)
            {
                var viewLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
                if (viewLocation != null)
                {
                    return viewLocation;
                }
            }
            return !flag ? GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, themeName, key, ref searchedLocations)
                : GetPathFromSpecificName(controllerContext, name, key, ref searchedLocations);
        }

        protected static List<ViewLocation> GetViewLocations(IEnumerable<string> viewLocationFormats, IEnumerable<string> areaViewLocationFormats)
        {
            List<ViewLocation> list = new List<ViewLocation>();
            if (areaViewLocationFormats != null)
            {
                list.AddRange(from areaViewLocationFormat in areaViewLocationFormats
                    select new AreaAwareViewLocation(areaViewLocationFormat));
            }
            if (viewLocationFormats != null)
            {
                list.AddRange(from viewLocationFormat in viewLocationFormats
                    select new ViewLocation(viewLocationFormat));
            }
            return list;
        }

        protected static bool IsSpecificPath(string name)
        {
            var ch = name[0];
            if (ch != '~')
            {
                return (ch == '/');
            }
            return true;
        }

        protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string themeName)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}:", new object[]
            {
                base.GetType().AssemblyQualifiedName,
                themeName,
                prefix,
                name,
                controllerName,
                areaName
            });
        }

        protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, IList<ViewLocation> locations, string name, string controllerName, string areaName, string themeName, string cacheKey, ref string[] searchedLocations)
        {
            var virtualPath = string.Empty;
            searchedLocations = new string[locations.Count];
            for (var i = 0; i < locations.Count; i++)
            {
                var str2 = locations[i].Format(name, controllerName, areaName, themeName);

                if (FileExists(controllerContext, str2))
                {
                    searchedLocations = EmptyLocations;
                    virtualPath = str2;
                    ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
                    return virtualPath;
                }
                searchedLocations[i] = str2;
            }
            return virtualPath;
        }

        protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            var virtualPath = name;
            if (!FileExists(controllerContext, name))
            {
                virtualPath = string.Empty;
                searchedLocations = new[] { name };
            }
            ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
            return virtualPath;
        }

        protected partial class ViewLocation
        {
            public ViewLocation(string virtualPathFormatString)
            {
                this.VirtualPathFormatString = virtualPathFormatString;
            }

            public virtual string Format(string viewName, string controllerName, string areaName, string theme)
            {
                return string.Format(CultureInfo.InvariantCulture, this.VirtualPathFormatString, new object[]
                {
                    viewName,
                    controllerName,
                    theme
                });
            }

            protected readonly string VirtualPathFormatString;
        }

        protected partial class AreaAwareViewLocation : ViewLocation
        {
            public AreaAwareViewLocation(string virtualPathFormatString) : base(virtualPathFormatString)
            {
            }

            public override string Format(string viewName, string controllerName, string areaName, string theme)
            {
                return string.Format(CultureInfo.InvariantCulture, VirtualPathFormatString, new object[]
                {
                    viewName,
                    controllerName,
                    areaName,
                    theme
                });
            }
        }
    }
    
}