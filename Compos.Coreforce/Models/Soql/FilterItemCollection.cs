using System.Collections.Generic;
using System.Linq;

namespace Compos.Coreforce.Models.Soql
{
    public class FilterItemCollection<T> : IFilterItem<T>
    {
        public List<IFilterItem<T>> Collection { get; set; }

        public FilterItemCollection(params IFilterItem<T>[] collection)
        {
            Collection = collection.ToList();
        }

        public string BuildStatement()
        {
            string whereClause = "+(";

            foreach (var whereItem in Collection)
            {
                whereClause = $"{whereClause}{whereItem.BuildStatement()}";
            }

            whereClause = $"{whereClause}+)";
            return whereClause;
        }
    }
}
