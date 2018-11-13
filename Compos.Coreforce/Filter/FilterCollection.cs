using System.Collections.Generic;
using System.Text;

namespace Compos.Coreforce.Filter
{
    public class FilterCollection : IFilter
    {
        private readonly IList<IFilter> filterCollection;

        public FilterCollection(List<IFilter> filterCollection)
        {
            this.filterCollection = filterCollection;
        }

        public string Get()
        {
            StringBuilder filter = new StringBuilder();

            foreach(var filterItem in filterCollection)
            {
                filter.Append(filterItem.Get());
            }

            return filter.ToString();
        }
    }
}
