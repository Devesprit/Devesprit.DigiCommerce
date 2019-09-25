using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;
using Mapster;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class FileServerModelFactory : IFileServerModelFactory
    {
        public virtual FileServerModel PrepareFileServerModel(TblFileServers fileServer)
        {
            var result = fileServer == null ? new FileServerModel() : fileServer.Adapt<FileServerModel>();
            return result;
        }

        public virtual TblFileServers PrepareTblFileServers(FileServerModel fileServer)
        {
            var result = fileServer.Adapt<TblFileServers>();
            return result;
        }
    }
}