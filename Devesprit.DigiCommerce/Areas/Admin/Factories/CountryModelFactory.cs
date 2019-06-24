using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

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
                result = Mapper.Map<CountryModel>(country);
                await country.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblCountries PrepareTblCountries(CountryModel country)
        {
            var result = Mapper.Map<TblCountries>(country);
            return result;
        }
    }
}