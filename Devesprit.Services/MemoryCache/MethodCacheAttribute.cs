using System;

namespace Devesprit.Services.MemoryCache
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodCacheAttribute : Attribute
    {
        public bool Enabled { get; set; } = true;
        public int DurationSec { get; set; } = 3600;
        public string VaryByParam { get; set; } = "*";
        public string VaryByCustom { get; set; } = null;
        public string[] Tags { get; set; } = null;
        public bool DoNotCacheForAdminUser { get; set; } = false;
    }
}