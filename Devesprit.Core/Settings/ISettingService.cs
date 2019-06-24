using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Devesprit.Data.Domain;

namespace Devesprit.Core.Settings
{
    public partial interface ISettingService
    {
        T FindByKey<T>(string key, T defaultValue = default(T));

        T LoadSetting<T>() where T : ISettings, new();

        Task<T> LoadSettingAsync<T>() where T : ISettings, new();

        Task SaveSettingAsync<T>(T settings) where T : ISettings, new();

        void SaveSetting<T>(T settings) where T : ISettings, new();
        
        void DeleteSetting<T>() where T : ISettings, new();
    }
}