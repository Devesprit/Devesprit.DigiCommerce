using System;
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
using Devesprit.Services.MemoryCache;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Languages
{
    public partial class LanguagesService : ILanguagesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;


        public LanguagesService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual TblLanguages GetDefaultLanguage()
        {
            return _dbContext.Languages
                       .Include(p => p.DefaultCurrency)
                       .DeferredFirstOrDefault(p => p.IsDefault && p.Published)
                       .FromCache(new []
                       {
                           QueryCacheTag.Language, QueryCacheTag.Currency
                       }) ??
                   _dbContext.Languages
                       .Include(p => p.DefaultCurrency)
                       .DeferredFirstOrDefault(p => p.Published)
                       .FromCache(new []
                       {
                           QueryCacheTag.Language, QueryCacheTag.Currency
                       });
        }
        
        public virtual List<SelectListItem> GetAsSelectList()
        {
            return GetAsEnumerable()
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.LanguageName })
                .ToList();
        }

        public virtual async Task<TblLanguages> GetDefaultLanguageAsync()
        {
            var result = await _dbContext.Languages
                             .DeferredFirstOrDefault(p => p.IsDefault && p.Published)
                             .FromCacheAsync(QueryCacheTag.Language) ??
                         await _dbContext.Languages
                             .DeferredFirstOrDefault(p => p.Published)
                             .FromCacheAsync(QueryCacheTag.Language);
            return result;
        }

        public virtual TblLanguages FindByIso(string iso)
        {
            return _dbContext.Languages
                    .Include(p => p.DefaultCurrency)
                    .DeferredFirstOrDefault(p => p.IsoCode.Trim().ToLower() == iso.Trim().ToLower())
                    .FromCache(new[] { QueryCacheTag.Language, QueryCacheTag.Currency });
        }

        public virtual async Task<TblLanguages> FindByIsoAsync(string iso)
        {
            var result = await _dbContext.Languages
                .DeferredFirstOrDefault(p => p.IsoCode.Trim().ToLower() == iso.Trim().ToLower())
                .FromCacheAsync(QueryCacheTag.Language);
            return result;
        }

        public virtual TblLanguages FindById(int id)
        {
            return _dbContext.Languages.Include(p=> p.DefaultCurrency)
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCache(QueryCacheTag.Language);
        }

        public virtual List<string> GetAllLanguagesIsoList()
        {
            return _dbContext.Languages.Where(p => p.Published)
                .Select(p => p.IsoCode)
                .FromCache(QueryCacheTag.Language).Select(p => p.ToLower().Trim()).ToList();
        }

        public virtual IEnumerable<TblLanguages> GetAsEnumerable()
        {
            return _dbContext.Languages.Where(p => p.Published).OrderBy(p => p.DisplayOrder)
                .FromCache(QueryCacheTag.Language);
        }

        public virtual IQueryable<TblLanguages> GetAsQueryable()
        {
            return _dbContext.Languages.OrderBy(p => p.DisplayOrder);
        }

        public virtual async Task<TblLanguages> FindByIdAsync(int id)
        {
            var result = await _dbContext.Languages
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(QueryCacheTag.Language);
            return result;
        }

        public virtual async Task<List<string>> GetAllLanguagesIsoListAsync()
        {
            var result = await _dbContext.Languages.Where(p => p.Published).Select(p => p.IsoCode.ToLower().Trim())
                .FromCacheAsync(QueryCacheTag.Language);
            return result.ToList();
        }

        public virtual async Task<IEnumerable<TblLanguages>> GetAsEnumerableAsync()
        {
            var result = await _dbContext.Languages.Where(p => p.Published).OrderBy(p => p.DisplayOrder)
                .FromCacheAsync(QueryCacheTag.Language);
            return result;
        }
        
        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            if (record != null && record.IsDefault)
            {
                throw new Exception($"You can not delete system default language (Id: {id}).");
            }

            await _dbContext.Languages.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(record);
            QueryCacheManager.ExpireTag(QueryCacheTag.Language);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblLanguages record)
        {
            if (await _dbContext.Languages
                .DeferredAny(p => p.IsoCode.Trim() == record.IsoCode.Trim() && p.Id != record.Id)
                .FromCacheAsync(QueryCacheTag.Language))
            {
                throw new Exception($"The \"{record.IsoCode}\" ISO code already exist.");
            }

            var oldRecord = await FindByIdAsync(record.Id);

            if (oldRecord != null)
            {
                if (oldRecord.IsDefault && !record.Published)
                {
                    throw new Exception("System default language cannot be at unpublished status.");
                }

                if (record.Icon == null || record.Icon.Length == 0)
                {
                    record.Icon = oldRecord.Icon;
                }
                _dbContext.Languages.AddOrUpdate(record);
                await _dbContext.SaveChangesAsync();

                QueryCacheManager.ExpireTag(QueryCacheTag.Language);

                _eventPublisher.EntityUpdated(record, oldRecord);
            }
        }

        public virtual async Task<int> AddAsync(TblLanguages record)
        {
            if (await _dbContext.Languages.DeferredAny(p => p.IsoCode.Trim() == record.IsoCode.Trim())
                .FromCacheAsync(QueryCacheTag.Language))
            {
                throw new Exception($"The \"{record.IsoCode}\" ISO code already exist.");
            }

            _dbContext.Languages.Add(record);
            await _dbContext.SaveChangesAsync();
            
            QueryCacheManager.ExpireTag(QueryCacheTag.Language);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public virtual async Task SetAsDefaultAsync(int id)
        {
            var record = await FindByIdAsync(id);
            if (record == null)
            {
                throw new Exception("Invalid language Id.");
            }
            if (!record.Published)
            {
                throw new Exception("Unpublished language cannot be used as the default language.");
            }

            await _dbContext.Languages
                .UpdateAsync(p => new TblLanguages() {IsDefault = false});
            await _dbContext.Languages.Where(p => p.Id == id)
                .UpdateAsync(p => new TblLanguages() {IsDefault = true});

            QueryCacheManager.ExpireTag(QueryCacheTag.Language);

            _eventPublisher.Publish(new DefaultLanguageChangeEvent(record));
        }
    }
}