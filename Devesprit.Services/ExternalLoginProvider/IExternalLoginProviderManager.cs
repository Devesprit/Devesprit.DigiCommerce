using System.Collections.Generic;
using Owin;

namespace Devesprit.Services.ExternalLoginProvider
{
    public partial interface IExternalLoginProviderManager
    {
        void RegisterExternalLoginProviders(IAppBuilder app);
        IExternalLoginProvider FindByProviderName(string providerName);
        List<ExternalLoginProviderInfo> GetAvailableLoginProvidersInfo();
    }
}