using System.Threading.Tasks;
using System.Web;

namespace Devesprit.FileServer.Repository.Interfaces
{
    public partial interface IFileManagerRepository
    {
        Task LogDownloadRequest(string filePath, string downloadQueryString, HttpContext currentContext);
        Task<int> GetDownloadCount(string request);
    }
}