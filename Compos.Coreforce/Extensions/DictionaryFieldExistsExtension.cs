using System.Collections.Generic;

namespace Compos.Coreforce.Extensions
{
    public static class DictionaryFieldExistsExtension
    {
        public static bool FieldExists<Value>(this Dictionary<string, Value> dictionary, string field)
        {
            try
            {
                if (dictionary[field] != null)
                    return true;
            }
            catch { }

            return false;
        }
    }
}
