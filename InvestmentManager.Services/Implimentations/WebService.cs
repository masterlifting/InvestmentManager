using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels.OutsideRequestModels;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Implimentations
{
    public class WebService : IWebService
    {
        private readonly HttpClient httpClient;
        public WebService(HttpClient httpClient) => this.httpClient = httpClient;

        public async Task<HttpResponseMessage> GetDataAsync(string query)
        {
            await Task.Delay(500).ConfigureAwait(false);
            var request = new HttpRequestMessage(HttpMethod.Get, query);
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<CBRF> GetDollarRateAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.cbr-xml-daily.ru/daily_json.js");
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<CBRF>(content);
        }
    }
}
