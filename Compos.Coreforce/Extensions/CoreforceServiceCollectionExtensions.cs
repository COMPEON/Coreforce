using Compos.Coreforce.Models.Authorization;
using Compos.Coreforce.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Linq;
using System;
using Compos.Coreforce.Authorization;

namespace Compos.Coreforce
{
    public static class SalesforceServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreforce(
            this IServiceCollection services, 
            OpenAuthorizationCredentials credentials, 
            string apiVersion = "v40.0", 
            int numberOfParallelThreads = 10
            )
        {
            // Set credentials
            CoreforceConfiguration.AuthorizationUrl = credentials.AuthorizationUrl;
            CoreforceConfiguration.ClientId = credentials.ClientId;
            CoreforceConfiguration.ClientSecret = credentials.ClientSecret;
            CoreforceConfiguration.GrantType = credentials.GrantType;
            CoreforceConfiguration.Password = credentials.Password;
            CoreforceConfiguration.Username = credentials.Username;
            CoreforceConfiguration.ApiVersion = apiVersion;
            CoreforceConfiguration.MaxNumberOfParallelRunningTasks = numberOfParallelThreads;

            services.AddTransient<ISalesforceOpenAuthorization, SalesforceOpenAuthorization>();
            services.AddTransient(typeof(ISalesforceRepository<>), typeof(SalesforceRepository<>));

            return services;
        }
    }
}
