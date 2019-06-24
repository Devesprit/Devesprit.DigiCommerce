using System;
using System.Linq.Expressions;
using System.Reflection;
using Devesprit.Core.Settings;

namespace Devesprit.Services.Settings
{
    public static partial class SettingExtensions
    {
        public static string GetSettingKey<T, TPropType>(this T entity,
            Expression<Func<T, TPropType>> keySelector)
            where T : ISettings, new()
        {
            var member = keySelector.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");
            }

            var key = typeof(T).Name + "." + propInfo.Name;
            return key;
        }
    }
}
