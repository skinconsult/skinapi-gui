using System.Net.Http;
using System.Threading.Tasks;

namespace SkinApi.Gui.Clients
{
    public class SkinApiClient : ISkinApiClient
    {
        private readonly HttpClient _httpClient;

        public SkinApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<HttpResponseMessage> GetTest()
        {
            return _httpClient.GetAsync("test");
        }
    }
}