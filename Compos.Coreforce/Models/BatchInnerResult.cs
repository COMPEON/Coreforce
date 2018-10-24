using Newtonsoft.Json;
using System.Collections.Generic;

namespace Compos.Coreforce.Models
{
    public class BatchInnerResult
    {
        [JsonProperty("result")]
        public List<BatchErrorResult> Result { get; set; }
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        public BatchInnerResult()
        {
            Result = new List<BatchErrorResult>();
        }
    }
}
