using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class SObjectTreeResult
    {
        [JsonProperty("hasErrors")]
        public bool HasErrors { get; set; }
        [JsonProperty("results")]
        public List<SObjectTreeInnerResult> Results { get; set; }

        public SObjectTreeResult()
        {
            HasErrors = false;
            Results = new List<SObjectTreeInnerResult>();
        }
    }
}
