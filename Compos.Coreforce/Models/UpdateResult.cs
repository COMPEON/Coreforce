using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class UpdateResult
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("fields")]
        public List<string> Fields { get; set; }
    }
}
