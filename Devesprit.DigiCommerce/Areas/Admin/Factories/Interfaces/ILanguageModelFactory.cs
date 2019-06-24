using System.Threading.Tasks;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces
{
    public partial interface ILanguageModelFactory
    {
        Task<LanguageModel> PrepareLanguageModelAsync(TblLanguages language);
        TblLanguages PrepareTblLanguages(LanguageModel language);
    }
}