using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Core.Localization
{
    public partial interface ILocalizationService
    {
        IQueryable<TblLocalizedStrings> GetAsQueryable();

        void Delete(int id);

        Task<TblLocalizedStrings> FindByIdAsync(int id);
        TblLocalizedStrings FindById(int id);

        TblLocalizedStrings FindByName(string resourceName, int languageId, bool logIfNotFound = false);

        int Add(TblLocalizedStrings record);

        void Update(TblLocalizedStrings record);

        string GetResource(string resourceName);

        string GetResource(string resourceName, int languageId, string defaultValue = "",
            bool returnEmptyIfNotFound = false, bool logIfNotFound = false);

        Task<IEnumerable<TblLocalizedStrings>> GetAsEnumerableAsync();
        IEnumerable<TblLocalizedStrings> GetAsEnumerable();

        Task<string> ExportResourcesToXmlAsync(TblLanguages language);

        Task ImportResourcesFromXmlAsync(int languageId, string xml, bool updateExistingResources = true);
    }
}