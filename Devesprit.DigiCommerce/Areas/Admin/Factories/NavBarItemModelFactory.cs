using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class NavBarItemModelFactory : INavBarItemModelFactory
    {
        public virtual async Task<NavBarItemModel> PrepareNavBarItemModelAsync(TblNavBarItems item)
        {
            NavBarItemModel result;
            if (item == null)
            {
                result = new NavBarItemModel();
            }
            else
            {
                result = Mapper.Map<NavBarItemModel>(item);
                await item.LoadAllLocalizedStringsToModelAsync(result);
            }
            
            return result;
        }

        public virtual TblNavBarItems PrepareTblNavBarItems(NavBarItemModel item)
        {
            var result = Mapper.Map<TblNavBarItems>(item);
            return result;
        }
    }
}