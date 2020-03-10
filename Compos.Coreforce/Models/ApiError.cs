using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Compos.Coreforce.Models
{
    public class ApiError
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("fields")]
        public List<string> Fields { get; set; }

        public ApiError()
        {
            Fields = new List<string>();
        }

        public override string ToString()
        {
            var result = $"ErrorCode: {ErrorCode}, Message: {Message}";

            if (Fields != null && Fields.Count > 0)
            {
                var fields = new StringBuilder();

                foreach (var field in Fields)
                    if (fields.Length > 0)
                        fields.Append($", {field}");
                    else
                        fields.Append(field);

                return $"{result}, Fields: {fields}";
            }

            return result;
        }
    }
}
