using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.NavBar
{
    public partial interface INavBarService
    {
        IQueryable<TblNavBarItems> GetAsQueryable();
        Task<TblNavBarItems> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblNavBarItems record);
        Task AddAsync(TblNavBarItems record);
        Task<IEnumerable<TblNavBarItems>> GetAsEnumerableAsync();
        IEnumerable<TblNavBarItems> GetAsEnumerable();
        Task SetNavbarItemsIndexAsync(int[] itemsOrder, int id, int? newParentId);
    }
}
