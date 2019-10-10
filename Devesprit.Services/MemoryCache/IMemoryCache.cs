using System;

namespace Devesprit.Services.MemoryCache
{
    public partial interface IMemoryCache
    {
        bool AddObject(string key, object obj, TimeSpan expire, string subKey = null);
        bool Contains(string key, string subKey = null);
        T GetObject<T>(string key, string subKey = null);
        object GetObject(string key, string subKey = null);
        T GetObjectOrDefault<T>(string key, T defaultValue, string subKey = null);
        void RemoveObject(string key, string subKey = null, bool removeSubKeys = true);
        void RemoveAllObjectsStartWithKey(string key);
        void RemoveAllObjectsContainKey(string key);
    }
}
