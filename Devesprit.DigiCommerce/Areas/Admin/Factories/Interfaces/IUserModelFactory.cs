using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface IUserModelFactory
    {
        Task<UserModel> PrepareUserModelAsync(TblUsers user);
        TblUsers PrepareTblUsers(UserModel user);
    }
}