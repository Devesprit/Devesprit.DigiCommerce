using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Pages
{
    public partial interface IPagesService
    {
        IQueryable<TblPages> GetAsQueryable();
        Task<TblPages> FindByIdAsync(int id);
        Task<TblPages> FindBySlugAsync(string slug);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPages record);
        Task<int> AddAsync(TblPages record);
        Task<IEnumerable<TblPages>> GetAsEnumerableAsync();
        IEnumerable<TblPages> GetPagesMustShowInFooter();
        IEnumerable<TblPages> GetPagesMustShowInUserMenuBar();
        Task<TblPages> GetWebsiteDefaultPageAsync();
    }
}
