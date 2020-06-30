using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;

namespace Devesprit.Services.MemoryCache
{
    public partial class MemoryCache : IMemoryCache, IDisposable
    {
        private static readonly CustomMemoryCache Cache = new CustomMemoryCache(Guid.NewGuid().ToString("N"));
        public MemoryCache()
        {}

        public virtual bool AddObject(string key, object obj, TimeSpan expire, string subKey = null)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = expire == TimeSpan.MaxValue ? DateTime.MaxValue : DateTime.Now.AddMilliseconds(expire.TotalMilliseconds)
            };
            return Cache.Add(RegionKey(key, subKey), obj, cacheItemPolicy);
        }

        public virtual bool Contains(string key, string subKey = null)
        {
            return Cache.Contains(RegionKey(key, subKey));
        }

        public virtual T GetObject<T>(string key, string subKey = null)
        {
            return (T)Cache.Get(RegionKey(key, subKey));
        }

        public object GetObject(string key, string subKey = null)
        {
            return Cache.Get(RegionKey(key, subKey));
        }

        public virtual T GetObjectOrDefault<T>(string key, T defaultValue, string subKey = null)
        {
            if (Cache.Contains(RegionKey(key, subKey)))
            {
                try
                {
                    return (T)Cache.Get(RegionKey(key, subKey));
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
                Cache.Remove(key);
                if (removeSubKeys)
                {
                    foreach (var cachetKey in Cache.GetAllKeys().Where(p => p.StartsWith(key + "::")))
                    {
                        Cache.Remove(cachetKey);
                    }
                }
            }
            else
            {
                Cache.Remove(RegionKey(key, subKey));
            }
        }

        public virtual void RemoveAllObjectsStartWithKey(string key)
        {
            foreach (var cachetKey in Cache.GetAllKeys().Where(p => p.StartsWith(key)))
            {
                Cache.Remove(cachetKey);
            }
        }
        
        public virtual void RemoveAllObjects()
        {
            foreach (var cachetKey in Cache.GetAllKeys())
            {
                Cache.Remove(cachetKey);
            }
        }

        public virtual void RemoveAllObjectsContainKey(string key)
        {
            foreach (var cachetKey in Cache.GetAllKeys().Where(p => p.Contains(key)))
            {
                Cache.Remove(cachetKey);
            }
        }

        public virtual void Dispose()
        {
            Cache.Dispose();
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