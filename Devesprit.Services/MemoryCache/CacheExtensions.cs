using System;
using System.Web.Mvc;

namespace Devesprit.Services.MemoryCache
{
    public static partial class CacheExtensions
    {
        public static T Get<T>(this IMemoryCache cacheManager, string key, Func<T> acquire)
        {
            return Get(cacheManager, key, 60, acquire);
        }

        public static T Get<T>(this IMemoryCache cacheManager, string key, int cacheTime, Func<T> acquire)
        {
            if (cacheManager.Contains(key))
            {
                return cacheManager.GetObject<T>(key);
            }

            var result = acquire();
            if (cacheTime > 0)
                cacheManager.AddObject(key, result, TimeSpan.FromSeconds(cacheTime));
            return result;
        }
    }
}
