using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Languages
{
    public partial interface ILanguagesService
    {
        TblLanguages GetDefaultLanguage();
        List<SelectListItem> GetAsSelectList();
        TblLanguages FindByIso(string iso);
        Task<TblLanguages> GetDefaultLanguageAsync();
        Task<TblLanguages> FindByIsoAsync(string iso);
        TblLanguages FindById(int id);
        List<string> GetAllLanguagesIsoList();
        IEnumerable<TblLanguages> GetAsEnumerable();
        IQueryable<TblLanguages> GetAsQueryable();
        Task<TblLanguages> FindByIdAsync(int id);
        Task<List<string>> GetAllLanguagesIsoListAsync();
        Task<IEnumerable<TblLanguages>> GetAsEnumerableAsync();
        Task DeleteAsync(int id);
        Task UpdateAsync(TblLanguages record);
        Task<int> AddAsync(TblLanguages record);
        Task SetAsDefaultAsync(int id);
    }
}
