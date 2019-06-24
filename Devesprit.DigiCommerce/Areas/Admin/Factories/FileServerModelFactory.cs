using AutoMapper;
using Devesprit.Data.Domain;
using Devesprit.DigiCommerce.Areas.Admin.Factories.Interfaces;
using Devesprit.DigiCommerce.Areas.Admin.Models;

namespace Devesprit.DigiCommerce.Areas.Admin.Factories
{
    public partial class FileServerModelFactory : IFileServerModelFactory
    {
        public virtual FileServerModel PrepareFileServerModel(TblFileServers fileServer)
        {
            var result = fileServer == null ? new FileServerModel() : Mapper.Map<FileServerModel>(fileServer);
            return result;
        }

        public virtual TblFileServers PrepareTblFileServers(FileServerModel fileServer)
        {
            var result = Mapper.Map<TblFileServers>(fileServer);
            return result;
        }
    }
}