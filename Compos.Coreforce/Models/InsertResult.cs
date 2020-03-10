using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class InsertResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("errors")]
        public List<ApiError> Errors { get; set; }
    }
}
