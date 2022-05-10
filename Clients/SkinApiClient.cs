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
            // Post is for adding, no Id because it's a new one that doesn't have an Id yet
            await _httpClient.PostAsJsonAsync($"/Company", inputModel);
        }

        public async void UpdateCompanyAsync(CompanyRecord companyRecord)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(companyRecord), Encoding.UTF8, "application/json");
            // Patch is for (partial) updating, pass the Id of the company we want to update
            // PatchAsJsonAsync isn't available before .net 7-preview3
            await _httpClient.PatchAsync($"/Company/{companyRecord.CompanyId}", httpContent);
        }
    }
}