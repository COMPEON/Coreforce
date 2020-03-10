using Compos.Coreforce.Cache;
using Compos.Coreforce.Models.Configuration;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Compos.Coreforce.Extensions
{
    internal static class ListAddRangeExtension
    {
        public static void AddCollection<T>(this List<T> list, IEnumerable<T> collection)
        {
            if (CoreforceConfiguration.IsCacheEnabled)
            {
                var id = string.Empty;

                if (typeof(T) == typeof(object))
                {
                    foreach(var item in collection)
                    {
                        if (TryGetFieldId(item, out id))
                            CacheHelper.AddOrUpdate(id, item);
                    }
                }

                else
                {
                    var idProperty = typeof(T).GetProperties().FirstOrDefault(x => x.Name.ToLower() == "id" && x.PropertyType == typeof(string));

                    if (idProperty != null)
                    {
                        foreach (var item in collection)
                        {
                            id = idProperty.GetValue(item)?.ToString();

                            if (id != null && !string.IsNullOrEmpty(id.ToString()))
                                CacheHelper.AddOrUpdate(id.ToString(), item);
                        }
                    }
                }
            }

            list.AddRange(collection);
        }

        private static bool TryGetFieldId(dynamic obj, out string id)
        {
            var dynamicObj = new Dictionary<string, object>(obj as ExpandoObject);
            var fieldsToCheck = new List<string>() { "id", "Id" };

            foreach(var fieldToCheck in fieldsToCheck)
            {
                try
                {
                    if (dynamicObj.FieldExists(fieldToCheck))
                    {
                        id = dynamicObj[fieldToCheck].ToString();
                        return true;
                    }
                }
                catch { }
            }

            id = string.Empty;
            return false;
        }
    }    
}
