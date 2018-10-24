using Newtonsoft.Json;

namespace Compos.Coreforce.Models
{
    public class BatchErrorResult
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
