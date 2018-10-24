using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class SObjectTreeError
    {
        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("fields")]
        public List<string> Fields { get; set; }

        public SObjectTreeError()
        {
            Fields = new List<string>();
        }
    }
}
