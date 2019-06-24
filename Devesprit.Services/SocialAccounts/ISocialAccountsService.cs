using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.SocialAccounts
{
    public partial interface ISocialAccountsService
    {
        IQueryable<TblSocialAccounts> GetAsQueryable();
        IEnumerable<TblSocialAccounts> GetAsEnumerable();
        Task DeleteAsync(int id);
        Task<TblSocialAccounts> FindByIdAsync(int id);
        Task<int> AddAsync(TblSocialAccounts record);
        Task UpdateAsync(TblSocialAccounts record);
    }
}
