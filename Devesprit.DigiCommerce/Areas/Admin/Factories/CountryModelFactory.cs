using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class CountryModelFactory : ICountryModelFactory
    {
        public virtual async Task<CountryModel> PrepareCountryModelAsync(TblCountries country)
        {
            CountryModel result;
            if (country == null)
            {
                result = new CountryModel();
            }
            else
            {
                result = country.Adapt<CountryModel>();
                await country.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblCountries PrepareTblCountries(CountryModel country)
        {
            var result = country.Adapt<TblCountries>();
            return result;
        }
    }
}