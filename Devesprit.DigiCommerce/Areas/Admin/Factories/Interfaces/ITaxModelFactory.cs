using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ITaxModelFactory
    {
        Task<TaxModel> PrepareTaxModelAsync(TblTaxes tax);
        TblTaxes PrepareTblTaxes(TaxModel tax);
    }
}