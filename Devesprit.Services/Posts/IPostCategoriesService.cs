using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostCategoriesService
    {
        IQueryable<TblPostCategories> GetAsQueryable();
        Task<TblPostCategories> FindByIdAsync(int id);
        Task<TblPostCategories> FindBySlugAsync(string slug);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostCategories record);
        Task<int> AddAsync(TblPostCategories record);
        Task<List<SelectListItem>> GetAsSelectListAsync();
        List<SelectListItem> GetAsSelectList();
        Task<IEnumerable<TblPostCategories>> GetAsEnumerableAsync();
        IEnumerable<TblPostCategories> GetAsEnumerable();
        IEnumerable<TblPostCategories> GetCategoriesMustShowInFooter();
        List<int> GetSubCategories(int categoryId, List<int> result = null);
    }
}
