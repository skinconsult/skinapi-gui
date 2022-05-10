using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using Newtonsoft.Json;

namespace SkinApi.Gui.Clients
{
    public class SkinApiClient : ISkinApiClient
    {
        private readonly HttpClient _httpClient;

        public SkinApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<CompanyRecord>> GetCompaniesAsync()
        {
            var result =  await _httpClient.GetAsync("/Company");
            return await result.Content.ReadFromJsonAsync<IEnumerable<CompanyRecord>>() ?? throw new InvalidOperationException();
        }

        public async void AddCompanyAsync(CompanyRecord inputModel)
        {
            string companyId = null;
            var httpContent = new StringContent(JsonConvert.SerializeObject(inputModel), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/Company/{companyId}", httpContent);
        }

        public async void UpdateCompanyAsync(CompanyRecord companyRecord)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(companyRecord), Encoding.UTF8, "application/json");
            await _httpClient.PostAsync("/Company/" + companyRecord.CompanyID, httpContent);
        }
    }
}