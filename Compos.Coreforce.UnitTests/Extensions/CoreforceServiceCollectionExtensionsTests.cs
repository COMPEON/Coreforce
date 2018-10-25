using Compos.Coreforce.Models.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Text;

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
                new OpenAuthorizationCredentials()
                {
                    AuthorizationUrl = "http://salesforceUrl",
                    ClientId = "ClientId",
                    ClientSecret = "ClientSecret",
                    GrantType = "password",
                    Password = "Password",
                    Username = "Username"
                }, "v40.0", 100);
            
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.AuthorizationUrl, "http://salesforceUrl");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.ClientId, "ClientId");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.ClientSecret, "ClientSecret");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.GrantType, "password");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.Password, "Password");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.Username, "Username");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.ApiVersion, "v40.0");
            Assert.AreEqual(Compos.Coreforce.Models.Configuration.CoreforceConfiguration.MaxNumberOfParallelRunningTasks, 100);
            Assert.IsTrue(services.Any(x => x.ServiceType == typeof(Compos.Coreforce.Authorization.ISalesforceOpenAuthorization)));
            Assert.IsTrue(services.Any(x => x.ServiceType == typeof(Compos.Coreforce.ISalesforceRepository<>)));
        }
    }
}
