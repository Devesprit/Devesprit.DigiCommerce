using System;
using System.Runtime.Caching;
using Devesprit.Core;
using Z.EntityFramework.Plus;

namespace Devesprit.Services
{
    public partial class StartupTask: IStartupTask
    {
        public virtual void Execute()
        {
            var options = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromHours(2),
            };
            QueryCacheManager.DefaultCacheItemPolicy = options;
        }

        public int Order => 0;
    }
}
