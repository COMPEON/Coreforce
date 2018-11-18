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
        // Only for tests
        protected HttpClient _client = null;

        // Call minimize object
        protected static readonly object _oAuthLock = new object();
        protected static AuthorizationResult _oAuthResult = null;
        protected static DateTime _dateTime = DateTime.Now;

        protected AuthorizationResult CurrentOAuthResult
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
            try
            {
                if (string.IsNullOrEmpty(CoreforceConfiguration.ApiVersion) ||
                    string.IsNullOrEmpty(CoreforceConfiguration.ClientId) ||
                    string.IsNullOrEmpty(CoreforceConfiguration.ClientSecret) ||
                    string.IsNullOrEmpty(CoreforceConfiguration.Password) ||
                    string.IsNullOrEmpty(CoreforceConfiguration.Username))
                    throw new CoreforceException(CoreforceError.SettingsException);

                var authorizationResult = CurrentOAuthResult;
                if (authorizationResult != null)
                    return authorizationResult;

                using (var client = _client ?? new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    using (var content =
                        new MultipartFormDataContent())
                    {
                        content.Add(new StringContent(CoreforceConfiguration.GrantType), "grant_type");
                        content.Add(new StringContent(CoreforceConfiguration.ClientId), "client_id");
                        content.Add(new StringContent(CoreforceConfiguration.ClientSecret), "client_secret");
                        content.Add(new StringContent(CoreforceConfiguration.Username), "username");
                        content.Add(new StringContent(CoreforceConfiguration.Password), "password");

                        using (
                           var message =
                               await client.PostAsync(CoreforceConfiguration.AuthorizationUrl, content))
                        {
                            var responseString = await message.Content.ReadAsStringAsync();
                            authorizationResult = JsonConvert.DeserializeObject<AuthorizationResult>(responseString);

                            if (message.StatusCode == HttpStatusCode.OK)
                            {
                                this.CurrentOAuthResult = authorizationResult;
                                return authorizationResult;
                            }

                            else
                                throw new CoreforceException(CoreforceError.AuthorizationBadRequest, $"Response: {responseString}");
                        }
                    }
                }
            }
            catch (CoreforceException)
            {
                throw;
            }
            catch(Exception exception)
            {
                throw new CoreforceException(CoreforceError.AuthorizationError, exception);
            }
        }
    }
}
