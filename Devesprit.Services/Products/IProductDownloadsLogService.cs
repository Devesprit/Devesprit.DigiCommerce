using System.Linq;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Services.Products
{
    public partial interface IProductDownloadsLogService
    {
        IQueryable<TblProductDownloadsLog> GetAsQueryable();
        Task<TblProductDownloadsLog> FindByIdAsync(int id);
        Task DeleteAsync(int id);
        Task AddAsync(TblProductDownloadsLog log);
        int GetNumberOfDownloads();
    }
}