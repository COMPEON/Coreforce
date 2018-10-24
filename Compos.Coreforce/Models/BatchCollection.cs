using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    class BatchCollection<T>
    {
        [JsonProperty("batchRequests")]
        public List<BatchComponent<T>> BatchRequests { get; set; }

        public BatchCollection()
        {
            BatchRequests = new List<BatchComponent<T>>();
        }
    }
}
