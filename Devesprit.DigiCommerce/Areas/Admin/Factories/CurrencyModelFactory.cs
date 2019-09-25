using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using JetBrains.Annotations;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class CurrencyModelFactory : ICurrencyModelFactory
    {
        public virtual async Task<CurrencyModel> PrepareCurrencyModelAsync([CanBeNull] TblCurrencies currency)
        {
            CurrencyModel result;
            if (currency == null)
            {
                result = new CurrencyModel();
            }
            else
            {
                result = currency.Adapt<CurrencyModel>();
                await currency.LoadAllLocalizedStringsToModelAsync(result);
            }
            return result;
        }

        public virtual TblCurrencies PrepareTblCurrencies(CurrencyModel currency)
        {
            var result = currency.Adapt<TblCurrencies>();
            return result;
        }
    }
}