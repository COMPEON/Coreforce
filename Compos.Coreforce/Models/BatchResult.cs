using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class BatchResult
    {
        [JsonProperty("hasErrors")]
        public bool HasErrors { get; set; }
        [JsonProperty("results")]
        public List<BatchInnerResult> Results { get; set; }

        public BatchResult()
        {
            Results = new List<BatchInnerResult>();
        }
    }
}
