using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac.Extras.DynamicProxy;
using Devesprit.Data.Domain;
using Devesprit.Services.MemoryCache;

namespace Devesprit.Services.Countries
{
    [Intercept(typeof(MethodCache))]
    public partial interface ICountriesService
    {
        [MethodCache(Tags = new[] { CacheTags.Country }, VaryByCustom = "lang")]
        Task<List<Country>> GetAsEnumerableAsync();
        [MethodCache(Tags = new[] { CacheTags.Country }, VaryByCustom = "lang")]
        IEnumerable<Country> GetAsEnumerable();
        [MethodCache(Tags = new[] { CacheTags.Country }, VaryByCustom = "lang")]
        Task<List<SelectListItem>> GetAsSelectListAsync();
        [MethodCache(Tags = new[] { CacheTags.Country }, VaryByCustom = "lang")]
        List<SelectListItem> GetAsSelectList();
        IQueryable<TblCountries> GetAsQueryable();
        Task<TblCountries> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblCountries record);
        Task<int> AddAsync(TblCountries record);
    }
}
