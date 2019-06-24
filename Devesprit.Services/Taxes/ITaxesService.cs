using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Taxes
{
    public partial interface ITaxesService
    {
        Task<IEnumerable<TblTaxes>> GetAsEnumerableAsync();
        IQueryable<TblTaxes> GetAsQueryable();
        Task DeleteAsync(int id);
        Task<TblTaxes> FindByIdAsync(int id);
        Task<int> AddAsync(TblTaxes record);
        Task UpdateAsync(TblTaxes record);
    }
}
