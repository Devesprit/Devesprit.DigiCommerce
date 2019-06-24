using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;

namespace Devesprit.Services.MemoryCache
{
    public partial class MemoryCache : IMemoryCache
    {
        private readonly CustomMemoryCache _cache;
        public MemoryCache()
        {
            _cache = new CustomMemoryCache(Guid.NewGuid().ToString("N"));
        }

        public virtual bool AddObject(string key, object obj, TimeSpan expire, string subKey = null)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expire == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(expire.TotalMilliseconds)
            };
            return _cache.Add(RegionKey(key, subKey), obj, cacheItemPolicy);
        }

        public virtual bool Contains(string key, string subKey = null)
        {
            return _cache.Contains(RegionKey(key, subKey));
        }

        public virtual T GetObject<T>(string key, string subKey = null)
        {
            return (T)_cache.Get(RegionKey(key, subKey));
        }

        public virtual T GetObjectOrDefault<T>(string key, T defaultValue, string subKey = null)
        {
            if (_cache.Contains(RegionKey(key, subKey)))
            {
                try
                {
                    return (T)_cache.Get(RegionKey(key, subKey));
                }
                catch
                { }
            }

            return defaultValue;
        }

        public virtual void RemoveObject(string key, string subKey = null, bool removeSubKeys = true)
        {
            if (string.IsNullOrEmpty(subKey))
            {
                _cache.Remove(key);
                if (removeSubKeys)
                {
                    foreach (var cachetKey in _cache.GetAllKeys().Where(p => p.StartsWith(key + "::")))
                    {
                        _cache.Remove(cachetKey);
                    }
                }
            }
            else
            {
                _cache.Remove(RegionKey(key, subKey));
            }
        }

        public virtual void Dispose()
        {
            _cache.Dispose();
        }

        protected virtual string RegionKey(string key, string subKey)
        {
            // NB Implements region as a suffix, for prefix, swap order in the format
            return string.IsNullOrEmpty(subKey) ? key : $"{key}::{subKey}";
        }

        protected partial class CustomMemoryCache : System.Runtime.Caching.MemoryCache
        {
            public CustomMemoryCache(string name, NameValueCollection config = null) : base(name, config)
            {
            }

            public virtual List<string> GetAllKeys()
            {
                var result = new List<string>();
                using (var en = GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        result.Add(en.Current.Key);
                    }
                }
                return result;
            }
        }
    }
}