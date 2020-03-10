using System.Reflection;

namespace Compos.Coreforce.Attributes
{
    internal static class AttributeReader
    {
        internal static string GetSalesforceObjectName<T>()
        {
            var attribute = typeof(T).GetCustomAttribute(typeof(SalesforceObject), false);

            if (attribute is null)
                return typeof(T).Name;

            return ((SalesforceObject)attribute).Name;
        }
    }
}
