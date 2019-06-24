using System;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Devesprit.Utilities.Extensions
{
    public static partial class ObjectExtensions
    {
        public static string ObjectToJson(this object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (sender, args) => { args.ErrorContext.Handled = true; }
            });
        }

        public static T JsonToObject<T>(this string json)
        {
            try
            {
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<T>(json);
            }
            catch
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        public static object JsonToObject(this string json, Type type)
        {
            try
            {
                var jss = new JavaScriptSerializer();
                return jss.Deserialize(json, type);
            }
            catch 
            {
                return JsonConvert.DeserializeObject(json, type);
            }
        }

        public static object JsonToObject(this string json)
        {
            try
            {
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<dynamic>(json);
            }
            catch
            {
                return JsonConvert.DeserializeObject(json);
            }
        }

        public static T GetFieldValue<T>(this object obj, string fieldName, T defaultValue)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public |
                                              BindingFlags.NonPublic |
                                              BindingFlags.Instance);
            var field = fields.FirstOrDefault(p => p.Name == fieldName);
            if (field != null)
            {
                return (T)field.GetValue(obj);
            }

            return defaultValue;
        }
    }
}
