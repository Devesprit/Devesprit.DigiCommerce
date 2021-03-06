using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.FileManagerServiceReference;
using Devesprit.Services.FileUploadServiceReference;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.FileServers
{
    public partial class FileServersService : IFileServersService
    {
        private readonly AppDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;

        public FileServersService(AppDbContext dbContext,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        public virtual async Task<IEnumerable<TblFileServers>> GetAsEnumerableAsync()
        {
            var result = await GetAsQueryable()
                .FromCacheAsync(CacheTags.FileServer);
            return result;
        }

        public virtual async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() {Value = p.Id.ToString(), Text = p.FileServerName})
                .ToList();
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            var result = GetAsQueryable()
                .FromCache(CacheTags.FileServer);

            return result
                .Select(p => new SelectListItem() {Value = p.Id.ToString(), Text = p.FileServerName})
                .ToList();
        }

        public virtual IQueryable<TblFileServers> GetAsQueryable()
        {
            return _dbContext.FileServers.OrderBy(p => p.FileServerName);
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.FileServers.Where(p=> p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.FileServer);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<TblFileServers> FindByIdAsync(int id)
        {
            var result = await _dbContext.FileServers
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.FileServer);
            return result;
        }

        public virtual async Task<int> AddAsync(TblFileServers record)
        {
            _dbContext.FileServers.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.FileServer);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task UpdateAsync(TblFileServers record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.FileServers.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.FileServer);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual FileManagerServiceClient GetWebService(TblFileServers fileServer)
        {
            var binding = new WSHttpBinding
            {
                Security =
                {
                    Mode = SecurityMode.TransportWithMessageCredential,
                    Message =
                    {
                        ClientCredentialType = MessageCredentialType.UserName,
                        EstablishSecurityContext = false
                    }
                },
                AllowCookies = true,
                MaxReceivedMessageSize = 100000000,
                MaxBufferPoolSize = 100000000,
                ReaderQuotas = {MaxArrayLength = 100000000, MaxStringContentLength = 100000000, MaxDepth = 32}
            };

            binding.OpenTimeout = binding.ReceiveTimeout =
                binding.CloseTimeout = binding.SendTimeout = TimeSpan.FromSeconds(25);

            var endPoint = new EndpointAddress(fileServer.FileServerUrl);
            var fileManager = new FileManagerServiceClient(binding, endPoint);
            if (fileManager.ClientCredentials != null)
            {
                fileManager.ClientCredentials.UserName.UserName = fileServer.ServiceUserName;
                fileManager.ClientCredentials.UserName.Password = fileServer.ServicePassword;
            }

            return fileManager;
        }

        public virtual FileUploadServiceClient GetFileUploadWebService(TblFileServers fileServer)
        {
            var binding = new BasicHttpBinding()
            {
                Security =
                {
                    Mode = BasicHttpSecurityMode.TransportWithMessageCredential,
                    Message =
                    {
                        ClientCredentialType = BasicHttpMessageCredentialType.UserName,
                        AlgorithmSuite = SecurityAlgorithmSuite.Default
                    }
                },
                AllowCookies = true,
                MaxReceivedMessageSize = 100000000,
                MaxBufferPoolSize = 100000000,
                MessageEncoding = WSMessageEncoding.Mtom,
                ReaderQuotas = { MaxArrayLength = 100000000, MaxStringContentLength = 100000000, MaxDepth = 32 }
            };

            binding.OpenTimeout = binding.ReceiveTimeout =
                binding.CloseTimeout = binding.SendTimeout = TimeSpan.FromSeconds(25);

            var endPoint =
                new EndpointAddress(fileServer.FileUploadServerUrl);
            var uploadWebService = new FileUploadServiceClient(binding, endPoint);
            if (uploadWebService.ClientCredentials != null)
            {
                uploadWebService.ClientCredentials.UserName.UserName = fileServer.ServiceUserName;
                uploadWebService.ClientCredentials.UserName.Password = fileServer.ServicePassword;
            }

            return uploadWebService;
        }
    }
}