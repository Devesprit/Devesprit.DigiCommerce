using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ICountryModelFactory
    {
        Task<CountryModel> PrepareCountryModelAsync(TblCountries country);
        TblCountries PrepareTblCountries(CountryModel country);
    }
}
