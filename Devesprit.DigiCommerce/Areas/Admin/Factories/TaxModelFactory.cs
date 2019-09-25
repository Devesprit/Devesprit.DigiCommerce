using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class TaxModelFactory : ITaxModelFactory
    {
        public virtual async Task<TaxModel> PrepareTaxModelAsync(TblTaxes tax)
        {
            TaxModel result;
            if (tax == null)
            {
                result = new TaxModel();
            }
            else
            {
                result = tax.Adapt<TaxModel>();
                await tax.LoadAllLocalizedStringsToModelAsync(result);
            }

            return result;
        }

        public virtual TblTaxes PrepareTblTaxes(TaxModel tax)
        {
            var result = tax.Adapt<TblTaxes>();
            return result;
        }
    }
}