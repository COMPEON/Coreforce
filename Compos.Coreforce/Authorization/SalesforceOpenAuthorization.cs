using Compos.Coreforce.Models.Authorization;
using Compos.Coreforce.Models.Configuration;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Compos.Coreforce.Authorization
{
    public class SalesforceOpenAuthorization : ISalesforceOpenAuthorization
    {
        // Credentials
        public OpenAuthorizationCredentials Credentials { get; set; }

        // Call minimize object
        private static readonly object _oAuthLock = new object();
        private static AuthorizationResult _oAuthResult = null;
        private static DateTime _dateTime = DateTime.Now;

        public SalesforceOpenAuthorization()
        {
            Credentials = new OpenAuthorizationCredentials()
            {
                AuthorizationUrl = CoreforceConfiguration.AuthorizationUrl,
                ClientId = CoreforceConfiguration.ClientId,
                ClientSecret = CoreforceConfiguration.ClientSecret,
                GrantType = CoreforceConfiguration.GrantType,
                Password = CoreforceConfiguration.Password,
                Username = CoreforceConfiguration.Username
            };
        }

        public AuthorizationResult CurrentOAuthResult
        {
            get
            {
                lock (_oAuthLock)
                {
                    if (_oAuthResult == null || _dateTime.ToUniversalTime() < DateTime.Now.AddHours(-1).ToUniversalTime())
                        return null;

                    return _oAuthResult;
                }
            }
            set
            {
                lock (_oAuthLock)
                {
                    if (_oAuthResult == null || _dateTime.ToUniversalTime() < DateTime.Now.AddHours(-1).ToUniversalTime())
                    {
                        _oAuthResult = value;
                        _dateTime = DateTime.Now;
                    }
                }
            }
        }

        public async Task<AuthorizationResult> Authorize()
        {
            var authorizationResult = CurrentOAuthResult;
            if (authorizationResult != null)
                return authorizationResult;

            try
            {
                using (var client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    using (var content =
                        new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(Credentials.GrantType), "grant_type");
                        content.Add(new StringContent(Credentials.ClientId), "client_id");
                        content.Add(new StringContent(Credentials.ClientSecret), "client_secret");
                        content.Add(new StringContent(Credentials.Username), "username");
                        content.Add(new StringContent(Credentials.Password), "password");

                        using (
                           var message =
                               await client.PostAsync(Credentials.AuthorizationUrl, content))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            authorizationResult = JsonConvert.DeserializeObject<AuthorizationResult>(responseString);

                            if (authorizationResult != null)
                            {
                                this.CurrentOAuthResult = authorizationResult;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return authorizationResult;
        }
    }
}
