using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data.Domain;
using Devesprit.Services.FileManagerServiceReference;

namespace Devesprit.Services.FileServers
{
    public partial interface IFileServersService
    {
        Task<IEnumerable<TblFileServers>> GetAsEnumerableAsync();
        Task<List<SelectListItem>> GetAsSelectListAsync();
        List<SelectListItem> GetAsSelectList();
        IQueryable<TblFileServers> GetAsQueryable();
        Task DeleteAsync(int id);
        Task<TblFileServers> FindByIdAsync(int id);
        Task<int> AddAsync(TblFileServers record);
        Task UpdateAsync(TblFileServers record);
        FileManagerServiceClient GetWebService(TblFileServers fileServer);
    }
}
