using Compos.Coreforce.Models.Authorization;
using Compos.Coreforce.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Compos.Coreforce.Authorization;

namespace Compos.Coreforce
{
    public static class SalesforceServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreforce(
            this IServiceCollection services, 
            Environment environment,
            string username,
            string password,
            string clientId,
            string clientSecret,
            string apiVersion = "v43.0"
            )
        {
            // Set credentials
            CoreforceConfiguration.ClientId = clientId;
            CoreforceConfiguration.ClientSecret = clientSecret;
            CoreforceConfiguration.GrantType = "password";
            CoreforceConfiguration.Password = password;
            CoreforceConfiguration.Username = username;
            CoreforceConfiguration.ApiVersion = apiVersion;

            if (environment == Environment.Production)
                CoreforceConfiguration.AuthorizationUrl = "https://login.salesforce.com/services/oauth2/token";

            if (environment == Environment.Sandbox)
                CoreforceConfiguration.AuthorizationUrl = "https://test.salesforce.com/services/oauth2/token";

            services.AddTransient<ISalesforceOpenAuthorization, SalesforceOpenAuthorization>();
            services.AddTransient(typeof(ISalesforceClient), typeof(SalesforceClient));

            return services;
        }
    }

    public enum Environment
    {
        Production,
        Sandbox
    }
}
