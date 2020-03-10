using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Compos.Coreforce.Extensions
{
    public static class HttpPatchExtension
    {
        public static async Task<HttpResponseMessage> PatchJsonAsync(this HttpClient client, string requestUri, string content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            return await client.SendAsync(request);
        }
    }
}
