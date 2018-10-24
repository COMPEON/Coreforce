namespace Compos.Coreforce.Models.Authorization
{
    public class OpenAuthorizationCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string AuthorizationUrl { get; set; }
        public string GrantType { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
