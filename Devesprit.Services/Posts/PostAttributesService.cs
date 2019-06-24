using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Posts
{
    public partial class PostAttributesService : IPostAttributesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public PostAttributesService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblPostAttributes> GetAsQueryable()
        {
            return _dbContext.PostAttributes;
        }

        public virtual async Task<TblPostAttributes> FindByIdAsync(int id)
        {
            var result = await _dbContext.PostAttributes
                .Include(p => p.Options)
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.PostAttribute);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.PostAttributes.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostAttributes).Name, id);
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblPostAttributes record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.PostAttributes.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddAsync(TblPostAttributes record)
        {
            _dbContext.PostAttributes.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual List<SelectListItem> GetAsSelectList()
        {
            var attributes = GetAsQueryable().FromCache(QueryCacheTag.PostAttribute).ToList();
            if (attributes.Any())
            {
                return attributes.Select(p => new SelectListItem()
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();
            }

            return new List<SelectListItem>();
        }


        #region Attribute Options

        public virtual IQueryable<TblPostAttributeOptions> GetOptionsAsQueryable(int? filterByAttributeId)
        {
            IQueryable<TblPostAttributeOptions> result = _dbContext.PostAttributeOptions;
            if (filterByAttributeId != null)
            {
                result = result.Where(p => p.PostAttributeId == filterByAttributeId);
            }
            return result;
        }

        public virtual async Task<TblPostAttributeOptions> FindOptionByIdAsync(int id)
        {
            var result = await _dbContext.PostAttributeOptions
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.PostAttribute);
            return result;
        }

        public virtual async Task DeleteOptionAsync(int id)
        {
            var record = await FindOptionByIdAsync(id);
            await _dbContext.PostAttributeOptions.Where(p => p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(typeof(TblPostAttributeOptions).Name,
                id);
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateOptionAsync(TblPostAttributeOptions record)
        {
            var oldRecord = await FindOptionByIdAsync(record.Id);
            _dbContext.PostAttributeOptions.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task<int> AddOptionAsync(TblPostAttributeOptions record)
        {
            _dbContext.PostAttributeOptions.Add(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(QueryCacheTag.PostAttribute);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual List<SelectListItem> GetOptionsAsSelectList(int attributeId)
        {
            var attribute = GetOptionsAsQueryable(attributeId).FromCache(QueryCacheTag.PostAttribute).ToList();
            if (attribute.Any())
            {
                return attribute.Select(p => new SelectListItem()
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();
            }

            return new List<SelectListItem>();
        }

        #endregion
    }
}