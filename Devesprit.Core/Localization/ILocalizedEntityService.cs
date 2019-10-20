using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Devesprit.Core.Settings;
using Devesprit.Data;
using Devesprit.Data.Domain;

namespace Devesprit.Core.Localization
{
    public partial interface ILocalizedEntityService
    {
        TblLocalizedProperty FindById(int localizedPropertyId);

        string GetLocalizedString(int languageId, int entityId, string localeKeyGroup, string localeKey);

        Task SaveLocalizedStringAsync<T>(T entity,
            Expression<Func<T, string>> keySelector,
            string localeValue,
            int languageId) where T : BaseEntity;

        Task SaveLocalizedStringAsync<T>(T entity,
            Expression<Func<T, string>> keySelector,
            LocalizedString localizedString) where T : BaseEntity;

        Task SaveAllLocalizedStringsAsync<T>(T entity, object model) where T : BaseEntity;

        Task SaveLocalizedStringAsync<T, TPropType>(T entity,
            Expression<Func<T, TPropType>> keySelector,
            TPropType localeValue,
            int languageId) where T : BaseEntity;

        Task SaveLocalizedSettingAsync<T>(T setting,
            string localeKeyGroup, string localeKey,
            LocalizedString localizedString) where T : ISettings;
        void SaveLocalizedSetting<T>(T setting,
            string localeKeyGroup, string localeKey,
            LocalizedString localizedString) where T : ISettings;

        Task DeleteEntityAllLocalizedStringsAsync<T>(T entity, int? languageId = null) where T : BaseEntity;
        void DeleteEntityAllLocalizedStrings<T>(T entity, int? languageId = null) where T : BaseEntity;
        Task DeleteEntityAllLocalizedStringsAsync(string localeKeyGroup, int entityId, int? languageId = null);
        void DeleteEntityAllLocalizedStrings(string localeKeyGroup, int entityId, int? languageId = null);
        void DeleteEntityAllLocalizedStrings(int entityId, string localeKeyGroup, string localKey, int? languageId = null);

        IList<TblLocalizedProperty> GetLocalizedProperties(int entityId, string localeKeyGroup);
        Task<IList<TblLocalizedProperty>> GetLocalizedPropertiesAsync(int entityId, string localeKeyGroup);
        IList<TblLocalizedProperty> GetLocalizedProperties(int entityId, string localeKeyGroup, string localKey);

        Task<IList<TblLocalizedProperty>> GetLocalizedPropertiesAsync(int entityId, string localeKeyGroup,
            string localKey);

        Task DeleteAsync(TblLocalizedProperty localizedProperty);
        void Delete(TblLocalizedProperty localizedProperty);
        Task AddAsync(TblLocalizedProperty localizedProperty);
        void Add(TblLocalizedProperty localizedProperty);
        Task UpdateAsync(TblLocalizedProperty localizedProperty);
        void Update(TblLocalizedProperty localizedProperty);
    }
}
