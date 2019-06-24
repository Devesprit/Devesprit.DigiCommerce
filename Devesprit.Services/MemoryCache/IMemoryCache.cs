using System;

namespace Devesprit.Services.MemoryCache
{
    public partial interface IMemoryCache
    {
        bool AddObject(string key, object obj, TimeSpan expire, string regionName = null);
        bool Contains(string key, string regionName = null);
        T GetObject<T>(string key, string regionName = null);
        T GetObjectOrDefault<T>(string key, T defaultValue, string regionName = null);
        void RemoveObject(string key, string regionName = null, bool removeSubKeys = true);
    }
}
