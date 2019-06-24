using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostTagsService
    {
        IQueryable<TblPostTags> GetAsQueryable();
        Task<IEnumerable<TblPostTags>> GetAsEnumerableAsync();
        Task<TblPostTags> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostTags record);
        Task<int> AddAsync(TblPostTags record);
        Task<List<TblPostTags>> TagSuggestionAsync(string query);
    }
}
