using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Countries
{
    public partial interface ICountriesService
    {
        Task<List<Country>> GetAsEnumerableAsync();
        IEnumerable<Country> GetAsEnumerable();
        Task<List<SelectListItem>> GetAsSelectListAsync();
        List<SelectListItem> GetAsSelectList();
        IQueryable<TblCountries> GetAsQueryable();
        Task<TblCountries> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblCountries record);
        Task<int> AddAsync(TblCountries record);
    }
}
