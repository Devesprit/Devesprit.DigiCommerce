using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using Castle.DynamicProxy;
using Microsoft.AspNet.Identity;

namespace Devesprit.Services.MemoryCache
{
    [Serializable]
    public class MethodCache : IInterceptor
    {
        private static readonly MemoryCache Cache = new MemoryCache();
        public delegate string VaryByCustomDelegate(string str);
        public static VaryByCustomDelegate GetVaryByCustom;
        private readonly bool _useCache;

        public MethodCache()
        {
            _useCache = !ConfigurationManager.AppSettings["DisableMemoryCache"].ToBooleanOrDefault(false);
        }

        public static void ExpireKey(string key, string subKey = null)
        {
            if (subKey != null)
            {
                Cache.RemoveObject(key, subKey, false);
                return;
            }

            Cache.RemoveObject(key);
        }

        public static void ExpireKeysStartWith(string key)
        {
            Cache.RemoveAllObjectsStartWithKey(key);
        }

        public static void ExpireTag(string tag)
        {
            Cache.RemoveAllObjectsContainKey(tag + ";");
        }

        public static void ExpireTags(string[] tags)
        {
            foreach (var tag in tags)
            {
                Cache.RemoveAllObjectsContainKey(tag + ";");
            }
        }

        private static string GenerateCacheKey(MethodInfo method, string[] tags)
        {
            var key = string.Empty;
            if (tags != null && tags.Length > 0)
            {
                key = string.Join(";", tags) + ";";
            }
            key += method.DeclaringType != null ? method.DeclaringType.FullName + "." : string.Empty;
            key += method.Name;
            return key;
        }

        private static string GenerateCacheSubKey(ParameterInfo[] arguments, object[] argumentValues,
            string varyByParam = "*", string varyByCustom = null)
        {
            if ((argumentValues == null || argumentValues.Length == 0) && varyByCustom == null)
                return null;

            string key = null;

            //VaryByParam
            if (argumentValues != null && argumentValues.Length > 0 && varyByParam != null)
            {
                if (varyByParam.Trim() == "*")
                {
                    foreach (var argument in argumentValues)
                    {
                        key += argument.IsNotNull() ? $"{argument}--" : "null--";
                    }
                }
                else
                {
                    var paramList = varyByParam.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        if (paramList.Contains(arguments[i].Name, StringComparer.InvariantCultureIgnoreCase))
                        {
                            key += argumentValues[i].IsNotNull() ? $"{argumentValues[i]}--" : "null--";
                        }
                    }
                }
            }

            //VaryByCustom
            if (!string.IsNullOrWhiteSpace(varyByCustom) && GetVaryByCustom != null)
            {
                var paramList = varyByCustom.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                key = paramList.Aggregate(key, (current, param) => current + GetVaryByCustom(param) + "--");
            }

            return key;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!_useCache || (invocation.Method.ReturnType == typeof(void) &&
                !invocation.Method.GetParameters().Any(p => p.ParameterType.IsByRef)))
            {
                invocation.Proceed();
                return;
            }

            var cacheAttr = invocation.Method.GetCustomAttributes<MethodCacheAttribute>(true).FirstOrDefault();
            if (cacheAttr == null || !cacheAttr.Enabled)
            {
                invocation.Proceed();
                return;
            }

            //Don't cache for Admin user
            if (cacheAttr.DoNotCacheForAdminUser)
            {
                var context = HttpContext.Current;
                if (context != null)
                {
                    if (context.User?.Identity?.IsAuthenticated == true)
                    {
                        if (context.User.IsInRole("Admin"))
                        {
                            invocation.Proceed();
                            return;
                        }
                    }
                }
            }

            var cacheKey = GenerateCacheKey(invocation.Method, cacheAttr.Tags);
            var cacheSubKey = GenerateCacheSubKey(invocation.Method.GetParameters(), invocation.Arguments,
                cacheAttr.VaryByParam, cacheAttr.VaryByCustom);
            MethodReturnValues methodReturnValues;

            if (Cache.Contains(cacheKey, cacheSubKey))
            {
                methodReturnValues = Cache.GetObject<MethodReturnValues>(cacheKey, cacheSubKey);
                invocation.ReturnValue = methodReturnValues.ReturnValue;
                if (methodReturnValues.OutParams != null && methodReturnValues.OutParams.Count > 0)
                {
                    foreach (var param in methodReturnValues.OutParams)
                    {
                        invocation.SetArgumentValue(param.Key, param.Value);
                    }
                }

                return;
            }


            invocation.Proceed();


            methodReturnValues = new MethodReturnValues()
            {
                ReturnValue = invocation.ReturnValue,
                OutParams = null
            };

            var parameters = invocation.Method.GetParameters();
            if (parameters.Any(p => p.ParameterType.IsByRef))
            {
                methodReturnValues.OutParams = new Dictionary<int, object>();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                    {
                        methodReturnValues.OutParams.Add(i, invocation.GetArgumentValue(i));
                    }
                }
            }

            Cache.AddObject(cacheKey, methodReturnValues, TimeSpan.FromSeconds(cacheAttr.DurationSec),
                cacheSubKey);
        }

        class MethodReturnValues
        {
            public object ReturnValue { get; set; }
            public Dictionary<int, object> OutParams { get; set; }
        }
    }
}