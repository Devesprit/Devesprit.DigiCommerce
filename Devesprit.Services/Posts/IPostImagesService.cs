using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostImagesService
    {
        Task<TblPostImages> FindByIdAsync(int id);
        IQueryable<TblPostImages> GetAsQueryable(int? filterByPostId);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostImages record);
        Task<int> AddAsync(TblPostImages record);
        
    }
}