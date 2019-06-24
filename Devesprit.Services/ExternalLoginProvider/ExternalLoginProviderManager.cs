using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Devesprit.Core.Plugin;
using Devesprit.Services.LicenseManager;
using Owin;

namespace Devesprit.Services.ExternalLoginProvider
{
    public partial class ExternalLoginProviderManager: IExternalLoginProviderManager
    {
        private readonly IPluginFinder _pluginFinder;

        public ExternalLoginProviderManager(IPluginFinder pluginFinder)
        {
            _pluginFinder = pluginFinder;
        }

        public virtual IExternalLoginProvider FindByProviderName(string providerName)
        {
            var providers = _pluginFinder.GetPlugins<IExternalLoginProvider>();
            foreach (var loginProvider in providers)
            {
                if (loginProvider.GetProviders().Any(p => p.ProviderName == providerName))
                {
                    return loginProvider;
                }
            }

            return null;
        }

        public virtual List<ExternalLoginProviderInfo> GetAvailableLoginProvidersInfo()
        {
            var result = new List<ExternalLoginProviderInfo>();
            var providers = _pluginFinder.GetPlugins<IExternalLoginProvider>();
            foreach (var loginProvider in providers)
            {
                result.AddRange(loginProvider.GetProviders());
            }

            return result;
        }

        public virtual void RegisterExternalLoginProviders(IAppBuilder app)
        {
            var providers = _pluginFinder.GetPlugins<IExternalLoginProvider>();

            foreach (var loginProvider in providers)
            {
                loginProvider.UseCustomLoginProvider(app);
            }
        }
    }
}
