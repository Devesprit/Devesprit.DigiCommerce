using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.FileManagerServiceReference;
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
                .FromCacheAsync(QueryCacheTag.FileServer);
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
                .FromCache(QueryCacheTag.FileServer);

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
            QueryCacheManager.ExpireTag(QueryCacheTag.FileServer);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task<TblFileServers> FindByIdAsync(int id)
        {
            var result = await _dbContext.FileServers
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.FileServer);
            return result;
        }

        public virtual async Task<int> AddAsync(TblFileServers record)
        {
            _dbContext.FileServers.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.FileServer);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task UpdateAsync(TblFileServers record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.FileServers.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.FileServer);

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
    }
}