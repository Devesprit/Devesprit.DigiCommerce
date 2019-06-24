using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ICurrencyModelFactory
    {
        Task<CurrencyModel> PrepareCurrencyModelAsync(TblCurrencies currency);
        TblCurrencies PrepareTblCurrencies(CurrencyModel currency);
    }
}