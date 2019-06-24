using System.Collections.Generic;
using Devesprit.Core.Plugin;
using Microsoft.AspNet.Identity.Owin;
using Owin;

namespace Devesprit.Services.ExternalLoginProvider
{
    public partial interface IExternalLoginProvider: IPlugin
    {
        void UseCustomLoginProvider(IAppBuilder app);
        ExternalLoginUserInformation GetUserInformation(ExternalLoginInfo loginInfo);
        List<ExternalLoginProviderInfo> GetProviders();
    }
}
