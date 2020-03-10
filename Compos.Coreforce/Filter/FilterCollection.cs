using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compos.Coreforce.Filter
{
    public class FilterCollection : IFilterCollection
    {
        public List<IFilter> Collection { get; set; }
        public int? Limit { get; set; }

        public FilterCollection(int limit, params IFilter[] filterCollection)
        {
            Collection = new List<IFilter>();
            Limit = limit;

            foreach (var item in filterCollection)
                Collection.Add(item);
        }

        public FilterCollection(params IFilter[] filterCollection)
        {
            Collection = new List<IFilter>();
            Limit = null;

            foreach (var item in filterCollection)
                Collection.Add(item);
        }

        public string Get()
        {
            StringBuilder filter = new StringBuilder();
            filter.Append("+(");

            foreach(var filterItem in Collection)
            {
                filter.Append(filterItem.Get());
            }

            filter.Append("+)");

            if (Limit is null)
                return filter.ToString();

            return filter.Append($"+LIMIT+{Limit}").ToString();
        }

        public void Add(IFilter filter)
        {
            Collection.Add(filter);
        }
    }
}
