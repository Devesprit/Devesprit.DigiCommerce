using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Localization;

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
                result = Mapper.Map<TaxModel>(tax);
                await tax.LoadAllLocalizedStringsToModelAsync(result);
            }

            return result;
        }

        public virtual TblTaxes PrepareTblTaxes(TaxModel tax)
        {
            var result = Mapper.Map<TblTaxes>(tax);
            return result;
        }
    }
}