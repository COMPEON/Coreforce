using Compos.Coreforce.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Compos.Coreforce.UnitTests.Authorization
{
    [TestClass]
    public class SalesforceOpenAuthorizationTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private SalesforceOpenAuthorizationTest _salesforceOpenAuthorization;

        [TestInitialize]
        public void Initialize()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _salesforceOpenAuthorization = new SalesforceOpenAuthorizationTest();
            _salesforceOpenAuthorization.SetClient(new HttpClient(_httpMessageHandlerMock.Object));

            Coreforce.Models.Configuration.CoreforceConfiguration.ApiVersion = "v43.0";
            Coreforce.Models.Configuration.CoreforceConfiguration.AuthorizationUrl = "http://www.coreforce-auth-test.com";
            Coreforce.Models.Configuration.CoreforceConfiguration.ClientId = "clientId";
            Coreforce.Models.Configuration.CoreforceConfiguration.ClientSecret = "clientSecret";
            Coreforce.Models.Configuration.CoreforceConfiguration.GrantType = "password";
            Coreforce.Models.Configuration.CoreforceConfiguration.Password = "password";
            Coreforce.Models.Configuration.CoreforceConfiguration.Username = "username";
        }

        [TestMethod]
        public void Authorize_CorrectCredentials_ReturnsFilledResultObject()
        {
            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                ).Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\": \"access\",\"instance_url\": \"url\",\"id\": \"id\",\"token_type\": \"Bearer\",\"issued_at\": \"123\",\"signature\": \"signature=\"}")
                }));

            var authorizationResult = _salesforceOpenAuthorization.Authorize().GetAwaiter().GetResult();

            Assert.AreEqual("access", authorizationResult.AccessToken);
            Assert.AreEqual("url", authorizationResult.InstanceUrl);
            Assert.AreEqual("id", authorizationResult.Id);
            Assert.AreEqual("Bearer", authorizationResult.TokenType);
            Assert.AreEqual("123", authorizationResult.IssuedAt);
            Assert.AreEqual("signature=", authorizationResult.Signature);
        }

        [TestMethod]
        public void Authorize_WrongCredentials_ReturnsNull()
        {
            _salesforceOpenAuthorization.ResetOAuthValue();

            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                ).Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\": \"invalid_grant\",\"error_description\": \"authentication failure\"}")
                }));

            Assert.ThrowsException<CoreforceException>(() => _salesforceOpenAuthorization.Authorize().GetAwaiter().GetResult());
        }

        [TestMethod]
        public void Authorize_NoCredentials_ThrowsCoreforceException()
        {
            Coreforce.Models.Configuration.CoreforceConfiguration.AuthorizationUrl = string.Empty;
            Coreforce.Models.Configuration.CoreforceConfiguration.ClientId = string.Empty;
            Coreforce.Models.Configuration.CoreforceConfiguration.ClientSecret = string.Empty;
            Coreforce.Models.Configuration.CoreforceConfiguration.GrantType = string.Empty;
            Coreforce.Models.Configuration.CoreforceConfiguration.Password = string.Empty;
            Coreforce.Models.Configuration.CoreforceConfiguration.Username = string.Empty;

            _httpMessageHandlerMock.Protected().Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
                ).Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\": \"invalid_grant\",\"error_description\": \"authentication failure\"}")
                }));

            Assert.ThrowsException<CoreforceException>(() => _salesforceOpenAuthorization.Authorize().GetAwaiter().GetResult());
        }
    }
    
    public class SalesforceOpenAuthorizationTest : SalesforceOpenAuthorization
    {
        public void SetClient(HttpClient client)
        {
            _client = client;
        }

        public void ResetOAuthValue()
        {
            _dateTime = DateTime.Now.AddHours(-2);
            CurrentOAuthResult = null;
        }
    }
}
