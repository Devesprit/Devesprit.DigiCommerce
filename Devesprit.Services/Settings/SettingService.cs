using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Devesprit.Core.Localization;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;
using Devesprit.Services.Events;
using Devesprit.Services.MemoryCache;
using Devesprit.Utilities.Extensions;
using Z.EntityFramework.Plus;

namespace Devesprit.Services.Settings
{
    public partial class SettingService : ISettingService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IMemoryCache _memoryCache;
        private readonly IEventPublisher _eventPublisher;

        public SettingService(AppDbContext dbContext, 
            ILocalizedEntityService localizedEntityService, 
            IMemoryCache memoryCache,
            IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _localizedEntityService = localizedEntityService;
            _memoryCache = memoryCache;
            _eventPublisher = eventPublisher;
        }

        protected virtual IEnumerable<TblSettings> GetAsEnumerable()
        {
            // Try get result from cache
            if (_memoryCache.Contains(CacheTags.Setting)) 
            {
                return _memoryCache.GetObject<List<TblSettings>>(CacheTags.Setting);
            }

            var result = _dbContext.Settings.FromCache(CacheTags.Setting).ToList(); 

            _memoryCache.AddObject(CacheTags.Setting, result, TimeSpan.FromDays(30));
            return result;
        }

        protected virtual async Task<int> AddAsync(TblSettings record)
        {
            _dbContext.Settings.Add(record);
            await _dbContext.SaveChangesAsync();
            ClearCache();

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        protected virtual async Task UpdateAsync(TblSettings record)
        {
            var oldRecord = FindById(record.Id);

            _dbContext.Settings.AddOrUpdate(record);
            await _dbContext.SaveChangesAsync();
            ClearCache();

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        protected virtual int Add(TblSettings record)
        {
            _dbContext.Settings.Add(record);
            _dbContext.SaveChanges();
            ClearCache();

            _eventPublisher.EntityInserted(record);

            return record.Id;
        }

        protected virtual void Update(TblSettings record)
        {
            var oldRecord = FindById(record.Id);

            _dbContext.Settings.AddOrUpdate(record);
            _dbContext.SaveChanges();
            ClearCache();

            _eventPublisher.EntityUpdated(record, oldRecord);
        }

        protected virtual void Delete(TblSettings record)
        {
            _dbContext.Settings.Where(p=> p.Id == record.Id).Delete();
            ClearCache();

            _eventPublisher.EntityDeleted(record);
        }

        protected virtual void Delete(IList<TblSettings> settings)
        {
            foreach (var setting in settings)
            {
                Delete(setting);
            }
        }

        protected virtual TblSettings FindById(int id)
        {
            var result = GetAsEnumerable().FirstOrDefault(p => p.Id == id);
            return result;
        }

        protected virtual TblSettings FindByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            key = key.Trim().ToLowerInvariant();
            var result = GetAsEnumerable().FirstOrDefault(p => p.Name == key);
            return result;
        }

        public virtual T FindByKey<T>(string key, T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var setting = FindByKey(key);
            if (setting != null)
            {
                return setting.Value.ToOrDefault<T>(defaultValue);
            }

            return defaultValue;
        }

        protected virtual async Task SetSettingAsync<T>(string key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = key.Trim().ToLowerInvariant();
            string valueStr = TypeDescriptor.GetConverter(typeof(T)).ConvertToInvariantString(value);

            var setting = FindByKey(key);
            if (setting != null)
            {
                //update
                setting.Value = valueStr;
                await UpdateAsync(setting);
            }
            else
            {
                //insert
                await AddAsync(new TblSettings(key, valueStr));
            }
        }

        protected virtual void SetSetting<T>(string key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            key = key.Trim().ToLowerInvariant();
            string valueStr = TypeDescriptor.GetConverter(typeof(T)).ConvertToInvariantString(value);

            var setting = FindByKey(key);
            if (setting != null)
            {
                //update
                setting.Value = valueStr;
                Update(setting);
            }
            else
            {
                //insert
                Add(new TblSettings(key, valueStr));
            }
        }

        protected virtual bool SettingExists<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector)
            where T : ISettings, new()
        {
            string key = settings.GetSettingKey(keySelector);

            var setting = FindByKey<string>(key);
            return setting != null;
        }

        public virtual T LoadSetting<T>() where T : ISettings, new()
        {
            // Try get result from cache
            if (_memoryCache.Contains(CacheTags.Setting, typeof(T).FullName))
            {
                return _memoryCache.GetObject<T>(CacheTags.Setting, typeof(T).FullName);
            }

            var settings = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(LocalizedString))
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    var setting = FindByKey<string>(key);
                    if (setting == null)
                        continue;

                    var value = new LocalizedString(setting);
                    foreach (var localizedProperty in _localizedEntityService.GetLocalizedProperties(0, typeof(T).Name, prop.Name))
                    {
                        value.Add(localizedProperty.LanguageId, localizedProperty.LocaleValue);
                    }
                    prop.SetValue(settings, value, null);
                }
                else
                {
                    var key = typeof(T).Name + "." + prop.Name;
                    var setting = FindByKey<string>(key);
                    if (setting == null)
                        continue;

                    if (TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    {
                        if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                            continue;

                        object value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                        prop.SetValue(settings, value, null);
                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType.IsSerializable)
                    {

                        try
                        {
                            var obj = setting.JsonToObject(prop.PropertyType);
                            prop.SetValue(settings, obj, null);
                        }
                        catch 
                        {
                        }
                    }
                }
            }

            _memoryCache.AddObject(CacheTags.Setting, settings, TimeSpan.FromDays(30), typeof(T).FullName);

            return settings;
        }

        public virtual async Task<T> LoadSettingAsync<T>() where T : ISettings, new()
        {
            // Try get result from cache
            if (_memoryCache.Contains(CacheTags.Setting, typeof(T).FullName))
            {
                return _memoryCache.GetObject<T>(CacheTags.Setting, typeof(T).FullName);
            }

            var settings = Activator.CreateInstance<T>();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(LocalizedString))
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    var setting = FindByKey<string>(key);
                    if (setting == null)
                        continue;

                    var value = new LocalizedString(setting);
                    foreach (var localizedProperty in await _localizedEntityService.GetLocalizedPropertiesAsync(0,
                        typeof(T).Name, prop.Name))
                    {
                        value.Add(localizedProperty.LanguageId, localizedProperty.LocaleValue);
                    }

                    prop.SetValue(settings, value, null);
                }
                else
                {
                    var key = typeof(T).Name + "." + prop.Name;
                    var setting = FindByKey<string>(key);
                    if (setting == null)
                        continue;

                    if (TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    {
                        if (!TypeDescriptor.GetConverter(prop.PropertyType).IsValid(setting))
                            continue;

                        object value = TypeDescriptor.GetConverter(prop.PropertyType).ConvertFromInvariantString(setting);

                        prop.SetValue(settings, value, null);
                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType.IsSerializable)
                    {

                        try
                        {
                            var obj = setting.JsonToObject(prop.PropertyType);
                            prop.SetValue(settings, obj, null);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            _memoryCache.AddObject(CacheTags.Setting, settings, TimeSpan.FromDays(30), typeof(T).FullName);

            return settings;
        }

        public virtual async Task SaveSettingAsync<T>(T settings) where T : ISettings, new()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(LocalizedString))
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    var value = prop.GetValue(settings, null).To<LocalizedString>();
                    if (value.ContainsKey(0))
                    {
                        await SetSettingAsync(key, value[0]);
                    }
                    else
                    {
                        await SetSettingAsync(key, "");
                    }
                    await _localizedEntityService.SaveLocalizedSettingAsync(settings, typeof(T).Name, prop.Name, value);
                }
                else
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    if (TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    {
                        var value = prop.GetValue(settings, null);
                        await SetSettingAsync(key, value ?? "");
                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType.IsSerializable)
                    {
                        var obj = prop.GetValue(settings, null);
                        var value = "";
                        if (obj != null)
                        {
                            try
                            {
                                value = obj.ObjectToJson();
                            }
                            catch
                            {}
                        }
                        await SetSettingAsync(key, value);
                    }
                }
            }
        }

        public virtual void SaveSetting<T>(T settings) where T : ISettings, new()
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(LocalizedString))
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    var value = prop.GetValue(settings, null).To<LocalizedString>();
                    if (value != null && value.ContainsKey(0))
                    {
                        SetSetting(key, value[0]);
                    }
                    else
                    {
                        SetSetting(key, "");
                    }
                    if (value != null)
                        _localizedEntityService.SaveLocalizedSetting(settings, typeof(T).Name, prop.Name, value);
                }
                else
                {
                    string key = typeof(T).Name + "." + prop.Name;
                    if (TypeDescriptor.GetConverter(prop.PropertyType).CanConvertFrom(typeof(string)))
                    {
                        var value = prop.GetValue(settings, null);
                        SetSetting(key, value ?? "");
                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType.IsSerializable)
                    {
                        var obj = prop.GetValue(settings, null);
                        var value = "";
                        if (obj != null)
                        {
                            try
                            {
                                value = obj.ObjectToJson();
                            }
                            catch
                            { }
                        }
                        SetSetting(key, value);
                    }
                }
            }
        }

        public virtual void DeleteSetting<T>() where T : ISettings, new()
        {
            var settingsToDelete = new List<TblSettings>();
            var allSettings = GetAsEnumerable();
            foreach (var prop in typeof(T).GetProperties())
            {
                string key = typeof(T).Name + "." + prop.Name;
                _localizedEntityService.DeleteEntityAllLocalizedStrings(0, typeof(T).Name, prop.Name);
                settingsToDelete.AddRange(allSettings.Where(x => x.Name.Equals(key, StringComparison.InvariantCultureIgnoreCase)));
            }

            Delete(settingsToDelete);
        }

        protected virtual void DeleteSetting<T, TPropType>(T settings,
            Expression<Func<T, TPropType>> keySelector) where T : ISettings, new()
        {
            var key = settings.GetSettingKey(keySelector);
            var setting = FindByKey(key);
            if (setting != null)
            {
                _localizedEntityService.DeleteEntityAllLocalizedStrings(0, typeof(T).Name, key);
                Delete(setting);
            }
        }

        protected virtual void ClearCache()
        {
            QueryCacheManager.ExpireTag(CacheTags.Setting);
            _memoryCache.RemoveObject(CacheTags.Setting);
        }
    }
}