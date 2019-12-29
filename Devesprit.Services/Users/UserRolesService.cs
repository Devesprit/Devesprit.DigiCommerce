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
using Devesprit.Utilities;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Users
{
    public partial class UserRolesService : IUserRolesService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IEventPublisher _eventPublisher;

        public UserRolesService(AppDbContext dbContext,
            ILocalizedEntityService localizedEntityService,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _eventPublisher = eventPublisher;
        }

        public async Task<IEnumerable<TblUserRoles>> GetAsEnumerableAsync()
        {
            var result = GetAsQueryable();
            return await result.FromCacheAsync(CacheTags.UserRoles);
        }

        public IEnumerable<TblUserRoles> GetAsEnumerable()
        {
            var result = GetAsQueryable();
            return result.FromCache(CacheTags.UserRoles);
        }

        public async Task<List<SelectListItem>> GetAsSelectListAsync()
        {
            return (await GetAsEnumerableAsync())
                .Select(p => new SelectListItem() { Value = p.Id.ToString(), Text = p.RoleName })
                .ToList();
        }

        public List<SelectListItem> GetAsSelectList()
        {
            return GetAsEnumerable().Select(p =>
                    new SelectListItem() {Value = p.Id.ToString(), Text = p.RoleName})
                .ToList();
        }

        public async Task<TblUserRoles> FindByIdAsync(int id)
        {
            var result = await _dbContext.UserRoles.Include(p=> p.Permissions).DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.UserRoles);
            return result;
        }

        public TblUserRoles FindById(int id)
        {
            var result = _dbContext.UserRoles.Include(p => p.Permissions).DeferredFirstOrDefault(p => p.Id == id)
                .FromCache(CacheTags.UserRoles);
            return result;
        }

        public IQueryable<TblUserRoles> GetAsQueryable()
        {
            return _dbContext.UserRoles.OrderBy(p => p.RoleName);
        }

        public async Task DeleteAsync(int id)
        {
            var record = await FindByIdAsync(id);
            await _dbContext.UserRoles.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityDeleted(record);
        }

        public async Task UpdateAsync(TblUserRoles record)
        {
            var oldRecord = await FindByIdAsync(record.Id);
            _dbContext.UserRoles.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public async Task<int> AddAsync(TblUserRoles record)
        {
            _dbContext.UserRoles.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public async Task<TblUserRolePermissions> FindPermissionByIdAsync(int id)
        {
            var result = await _dbContext.UserRolePermissions.DeferredFirstOrDefault(p => p.Id == id)
                .FromCacheAsync(CacheTags.UserRoles);
            return result;
        }

        public async Task DeleteRolePermissionAsync(int roleId)
        {
            var rolePermissionsId = (await FindByIdAsync(roleId))?.Permissions.Select(p=> p.Id).ToList() ?? null;
            if (rolePermissionsId != null && rolePermissionsId.Any())
            {
                await _dbContext.UserRolePermissions.Where(p => rolePermissionsId.Contains(p.Id)).DeleteAsync();
            }
            QueryCacheManager.ExpireTag(CacheTags.UserRoles);
        }

        public async Task DeletePermissionAsync(int id)
        {
            var record = await FindPermissionByIdAsync(id);
            await _dbContext.UserRolePermissions.Where(p => p.Id == id).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityDeleted(record);
        }

        public async Task UpdatePermissionAsync(TblUserRolePermissions record)
        {
            var oldRecord = await FindPermissionByIdAsync(record.Id);
            _dbContext.UserRolePermissions.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public async Task<int> AddPermissionAsync(TblUserRolePermissions record)
        {
            _dbContext.UserRolePermissions.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRoles);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public IQueryable<TblUserAccessAreas> GetUserAccessAreasAsQueryable()
        {
            return _dbContext.UserAccessAreas.OrderBy(p => p.AreaName);
        }

        public async Task<IEnumerable<TblUserAccessAreas>> GetUserAccessAreasAsEnumerableAsync()
        {
            var result = GetUserAccessAreasAsQueryable();
            return await result.FromCacheAsync(CacheTags.UserRolesAccessAreas);
        }

        public IEnumerable<TblUserAccessAreas> GetUserAccessAreasAsEnumerable()
        {
            var result = GetUserAccessAreasAsQueryable();
            return result.FromCache(CacheTags.UserRolesAccessAreas);
        }

        public async Task<TblUserAccessAreas> FindAccessAreasByNameAsync(string name)
        {
            var result = await _dbContext.UserAccessAreas.DeferredFirstOrDefault(p => p.AreaName == name)
                .FromCacheAsync(CacheTags.UserRolesAccessAreas);
            return result;
        }

        public TblUserAccessAreas FindAccessAreasByName(string name)
        {
            var result = _dbContext.UserAccessAreas.DeferredFirstOrDefault(p => p.AreaName == name)
                .FromCache(CacheTags.UserRolesAccessAreas);
            return result;
        }

        public async Task<int> AddAccessAreasAsync(TblUserAccessAreas record)
        {
            _dbContext.UserAccessAreas.Add(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public async Task DeleteAccessAreasAsync(string areaName)
        {
            var record = await _dbContext.UserAccessAreas.FirstOrDefaultAsync(p => p.AreaName == areaName);
            await _dbContext.UserAccessAreas.Where(p => p.AreaName == areaName || p.ParentAreaName == areaName).DeleteAsync();
            await _dbContext.UserRolePermissions.Where(p => p.AreaName == areaName).DeleteAsync();
            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityDeleted(record);
        }

        public async Task UpdateAccessAreasAsync(TblUserAccessAreas record)
        {
            var oldRecord = await _dbContext.UserAccessAreas.FirstOrDefaultAsync(p => p.Id == record.Id);
            _dbContext.UserAccessAreas.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();

            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public async Task GrantAllPermissionsToAdministratorAsync()
        {
            var adminRole = (await GetAsEnumerableAsync()).FirstOrDefault(p => p.RoleName == "Administrator");
            if (adminRole == null)
            {
                adminRole = new TblUserRoles()
                {
                    RoleName = "Administrator"
                };
                await AddAsync(adminRole);
            }
            await DeleteRolePermissionAsync(adminRole.Id);
            var accessAreas = GetUserAccessAreasAsEnumerable().ToList();
            _dbContext.UserRolePermissions.AddRange(accessAreas.Select(p => new TblUserRolePermissions()
            {
                AreaName = p.AreaName,
                HaveAccess = true,
                RoleId = adminRole.Id
            }));
            await _dbContext.SaveChangesAsync();
            QueryCacheManager.ExpireTag(CacheTags.UserRoles);
        }

        public int AddAccessAreas(TblUserAccessAreas record)
        {
            _dbContext.UserAccessAreas.Add(record);
            _dbContext.SaveChanges();

            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        public void DeleteAccessAreas(string areaName)
        {
            var record = _dbContext.UserAccessAreas.FirstOrDefault(p => p.AreaName == areaName);
            _dbContext.UserAccessAreas.Where(p => p.AreaName == areaName || p.ParentAreaName == areaName).Delete();
            _dbContext.UserRolePermissions.Where(p => p.AreaName == areaName).Delete();
            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityDeleted(record);
        }

        public void UpdateAccessAreas(TblUserAccessAreas record)
        {
            var oldRecord = _dbContext.UserAccessAreas.FirstOrDefault(p => p.Id == record.Id);
            _dbContext.UserAccessAreas.AddOrUpdate(record);
            _dbContext.SaveChanges();

            QueryCacheManager.ExpireTag(CacheTags.UserRolesAccessAreas);

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        public void GrantAllPermissionsToAdministrator()
        {
            var adminRole = GetAsEnumerable().FirstOrDefault(p => p.RoleName == "Administrator");
            if (adminRole == null)
            {
                adminRole = new TblUserRoles()
                {
                    RoleName = "Administrator"
                };
                AsyncHelper.RunSync(() => AddAsync(adminRole));
            }
            AsyncHelper.RunSync(() => DeleteRolePermissionAsync(adminRole.Id));
            var accessAreas = GetUserAccessAreasAsEnumerable().ToList();

            _dbContext.UserRolePermissions.AddRange(accessAreas.Select(p=> new TblUserRolePermissions()
            {
                AreaName = p.AreaName,
                HaveAccess = true,
                RoleId = adminRole.Id
            }));
            _dbContext.SaveChanges();
            QueryCacheManager.ExpireTag(CacheTags.UserRoles);
        }
    }
}