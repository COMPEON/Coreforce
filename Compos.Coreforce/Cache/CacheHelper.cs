using Compos.Coreforce.Models.Configuration;
using System;
using System.Dynamic;
using System.Runtime.Caching;

namespace Compos.Coreforce.Cache
{
    internal static class CacheHelper
    {
        public static void AddOrUpdate(string key, object value)
        {
            MemoryCache.Default.Set(key, value, DateTimeOffset.Now.AddSeconds(CoreforceConfiguration.CacheExpirationInSeconds));
        }

        public static T Get<T>(string key) where T : class
        {
            var value = MemoryCache.Default[key] as T;

            if (value != null && 
                (typeof(T) == value.GetType() || 
                (typeof(T) == typeof(object) && value.GetType() == typeof(ExpandoObject))
                ))
            {
                SalesforceClient.cnt++;
                return value;
            }

            return null;
        }
    }
}
