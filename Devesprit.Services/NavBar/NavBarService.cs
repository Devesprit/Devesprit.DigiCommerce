using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.NavBar
{
    public partial class NavBarService : INavBarService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public NavBarService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public virtual IQueryable<TblNavBarItems> GetAsQueryable()
        {
            return _dbContext.NavBarItems;
        }

        public virtual async Task<TblNavBarItems> FindByIdAsync(int id)
        {
            var result = await _dbContext.NavBarItems
                .DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.NavbarItem);
            return result;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);

            await _dbContext.NavBarItems.Where(p=> p.Id == id).DeleteAsync();
            await _localizedEntityService.DeleteEntityAllLocalizedStringsAsync(record);

            await _dbContext.NavBarItems.Where(p => p.Index > record.Index).UpdateAsync(p => new TblNavBarItems()
            {
                Index = p.Index - 1
            });

            QueryCacheManager.ExpireTag(CacheTags.NavbarItem);

            _eventPublisher.EntityDeleted(record);
        }

        public virtual async Task UpdateAsync(TblNavBarItems record)
        {
            var allItems = (await GetAsEnumerableAsync()).ToList();
            var oldRecord = allItems.FirstOrDefault(p => p.Id == record.Id);
            allItems.RemoveWhere(p => p.Id == record.Id);
            allItems.Add(record);
            if (DetectLoop(allItems, record, null))
                throw new Exception("Self referencing loop detected");

            _dbContext.NavBarItems.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.NavbarItem);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public virtual async Task AddAsync(TblNavBarItems record)
        {
            if (record.Index == -1)
            {
                var maxIdx = _dbContext.NavBarItems.Max(p => (int?)p.Index);
                record.Index = (maxIdx ?? 0) + 1;
            }
            else
            {
                await _dbContext.NavBarItems.Where(p => p.Index >= record.Index).UpdateAsync(p => new TblNavBarItems()
                {
                    Index = p.Index + 1
                });
            }

            _dbContext.NavBarItems.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.NavbarItem);

            _eventPublisher.EntityInserted(record);
        }

        public virtual async Task<IEnumerable<TblNavBarItems>> GetAsEnumerableAsync()
        {
            var result = await GetAsQueryable()
                .FromCacheAsync(CacheTags.NavbarItem);
            return result;
        }

        public virtual IEnumerable<TblNavBarItems> GetAsEnumerable()
        {
            var result = GetAsQueryable().FromCache(CacheTags.NavbarItem);
            return result;
        }

        public virtual async Task SetNavbarItemsIndexAsync(int[] itemsOrder, int id, int? newParentId)
        {
            var nodeList = await _dbContext.NavBarItems.ToListAsync();
            for (int i = 0; i < itemsOrder.Length; i++)
            {
                nodeList.First(p => p.Id == itemsOrder[i]).Index = i;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.NavBarItems.Where(p => p.Id == id).UpdateAsync(p => new TblNavBarItems()
            {
                ParentItemId = newParentId
            });

            QueryCacheManager.ExpireTag(CacheTags.NavbarItem);

            _eventPublisher.Publish(new NavbarItemsIndexChangeEvent(itemsOrder, id, newParentId));
        }

        protected virtual bool DetectLoop(List<TblNavBarItems> allItems, TblNavBarItems item, HashSet<TblNavBarItems> visited)
        {
            visited = visited ?? new HashSet<TblNavBarItems>();
            var parentItem = allItems.FirstOrDefault(p => p.Id == item.ParentItemId);
            if (parentItem != null)
            {
                if (!visited.Contains(parentItem))
                {
                    visited.Add(parentItem);
                    return DetectLoop(allItems, parentItem, visited);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}