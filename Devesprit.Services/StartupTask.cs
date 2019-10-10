using System;
using System.Configuration;
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
                SlidingExpiration = TimeSpan.FromHours(24),
            };
            QueryCacheManager.DefaultCacheItemPolicy = options;
            QueryCacheManager.IsEnabled = !ConfigurationManager.AppSettings["DisableSqlQueryCache"].ToBooleanOrDefault(false);
        }

        public int Order => 0;
    }
}
