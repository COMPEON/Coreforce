using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class SObjectTreeInnerResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }
        [JsonProperty("errors")]
        public List<SObjectTreeError> Errors { get; set; }

        public SObjectTreeInnerResult()
        {
            Errors = new List<SObjectTreeError>();
        }
    }
}
