using System.Threading.Tasks;
using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Devesprit.Services.Currency;
using JetBrains.Annotations;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class LanguageModelFactory : ILanguageModelFactory
    {
        private readonly ICurrencyService _currencyService;

        public LanguageModelFactory(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public virtual async Task<LanguageModel> PrepareLanguageModelAsync([CanBeNull] TblLanguages language)
        {
            var result = language == null ? new LanguageModel() : Mapper.Map<LanguageModel>(language);
            result.CurrenciesList = await _currencyService.GetAsSelectListAsync();
            return result;
        }

        public virtual TblLanguages PrepareTblLanguages(LanguageModel language)
        {
            var result = Mapper.Map<TblLanguages>(language);
            return result;
        }
    }
}