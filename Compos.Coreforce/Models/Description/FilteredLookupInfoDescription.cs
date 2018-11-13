using System.Collections.Generic;

namespace Compos.Coreforce.Models.Description
{
    public class FilteredLookupInfoDescription
    {
        public List<object> controllingFields { get; set; }
        public bool dependent { get; set; }
        public bool optionalFilter { get; set; }
    }
}
