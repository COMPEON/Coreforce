using Newtonsoft.Json;

namespace Compos.Coreforce.Models
{
    public class InsertResult : UpdateResult
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
