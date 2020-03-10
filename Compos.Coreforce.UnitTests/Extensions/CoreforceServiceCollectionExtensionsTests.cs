using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Compos.Coreforce.UnitTests.Extensions
{
    [TestClass]
    public class CoreforceServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void DependencyInjectionTest()
        {
            var services = new ServiceCollection();
            services.AddCoreforce(
                Environment.Production,
                "Username",
                "Password",
                "ClientId",
                "ClientSecret", 
                "v40.0"
                );
            
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.AuthorizationUrl, "https://login.salesforce.com/services/oauth2/token");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.ClientId, "ClientId");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.ClientSecret, "ClientSecret");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.GrantType, "password");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.Password, "Password");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.Username, "Username");
            Assert.AreEqual(Models.Configuration.CoreforceConfiguration.ApiVersion, "v40.0");
            Assert.IsTrue(services.Any(x => x.ServiceType == typeof(Compos.Coreforce.Authorization.ISalesforceOpenAuthorization)));
            Assert.IsTrue(services.Any(x => x.ServiceType == typeof(Compos.Coreforce.ISalesforceClient)));
        }
    }
}
