using System;

namespace Devesprit.Services.MemoryCache
{
    public static partial class MethodCache
    {
        private static MemoryCache _cache = new MemoryCache();
        public static T GetCachedResult<T>(this Func<T> func, string key, string subKey = null, int cacheTime = int.MaxValue)
        {
            if (_cache.Contains(key, subKey))
            {
                return _cache.GetObject<T>(key, subKey);
            }

            var result = func.Invoke();
            if (cacheTime > 0)
                _cache.AddObject(key, result, TimeSpan.FromSeconds(cacheTime), subKey);
            return result;
        }

        public static void ClearFromCache(string key, string subKey = null)
        {
            if (subKey != null)
            {
                _cache.RemoveObject(key, subKey, false);
                return;
            }
            
            _cache.RemoveObject(key);
        }
    }
}