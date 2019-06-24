using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ISocialAccountModelFactory
    {
        Task<SocialAccountModel> PrepareSocialAccountModelAsync(TblSocialAccounts socialAccount);
        TblSocialAccounts PrepareTblSocialAccounts(SocialAccountModel socialAccount);
    }
}