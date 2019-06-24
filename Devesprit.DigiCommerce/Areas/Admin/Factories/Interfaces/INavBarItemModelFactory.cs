using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface INavBarItemModelFactory
    {
        Task<NavBarItemModel> PrepareNavBarItemModelAsync(TblNavBarItems item);
        TblNavBarItems PrepareTblNavBarItems(NavBarItemModel item);
    }
}
