using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class SocialAccountModelFactory : ISocialAccountModelFactory
    {
        public virtual async Task<SocialAccountModel> PrepareSocialAccountModelAsync(TblSocialAccounts socialAccount)
        {
            SocialAccountModel result;
            if (socialAccount == null)
            {
                result = new SocialAccountModel();
            }
            else
            {
                result = socialAccount.Adapt<SocialAccountModel>();
                await socialAccount.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblSocialAccounts PrepareTblSocialAccounts(SocialAccountModel socialAccount)
        {
            var result = socialAccount.Adapt<TblSocialAccounts>();
            return result;
        }
    }
}