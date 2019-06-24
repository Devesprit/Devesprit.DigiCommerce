using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Posts
{
    public partial interface IPostAttributesMappingService
    {
        Task<TblPostAttributesMapping> FindByIdAsync(int id);
        IQueryable<TblPostAttributesMapping> GetAsQueryable(int productId);
        Task DeleteAsync(int id);
        Task UpdateAsync(TblPostAttributesMapping record);
        Task<int> AddAsync(TblPostAttributesMapping record);
    }
}