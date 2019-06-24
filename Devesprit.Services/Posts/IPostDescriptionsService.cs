using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostDescriptionsService
    {
        Task<TblPostDescriptions> FindByIdAsync(int id);
        IQueryable<TblPostDescriptions> GetAsQueryable(int? filterByPostId);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostDescriptions record);
        Task<int> AddAsync(TblPostDescriptions record);
    }
}