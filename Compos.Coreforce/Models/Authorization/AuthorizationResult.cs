using Newtonsoft.Json;

namespace Compos.Coreforce.Models.Authorization
{
    public class AuthorizationResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("instance_url")]
        public string InstanceUrl { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("issued_at")]
        public string IssuedAt { get; set; }
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
