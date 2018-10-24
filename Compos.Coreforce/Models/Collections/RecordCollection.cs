using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models.Collections
{
    public class RecordCollection<T>
        where T : new()
    {
        [JsonProperty("records")]
        public List<T> Records { get; set; }

        public RecordCollection()
        {
            Records = new List<T>();
        }

        public RecordCollection(List<T> objs)
        {
            Records = objs;
        }
    }
}
