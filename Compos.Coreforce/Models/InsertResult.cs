using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class InsertResult : List<ApiError>
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
