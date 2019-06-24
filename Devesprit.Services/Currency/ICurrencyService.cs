using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Currency
{
    public partial interface ICurrencyService
    {
        IEnumerable<TblCurrencies> GetAsEnumerable();
        IQueryable<TblCurrencies> GetAsQueryable();
        Task<IEnumerable<TblCurrencies>> GetAsEnumerableAsync();
        Task<List<SelectListItem>> GetAsSelectListAsync();
        TblCurrencies GetDefaultCurrency();
        Task<TblCurrencies> FindByIdAsync(int id);
        TblCurrencies FindByIso(string iso);
        List<string> GetAllCurrenciesIsoList();
        Task DeleteAsync(int id);
        Task UpdateAsync(TblCurrencies record);
        Task<int> AddAsync(TblCurrencies record);
        Task SetAsDefaultAsync(int id);
    }
}
