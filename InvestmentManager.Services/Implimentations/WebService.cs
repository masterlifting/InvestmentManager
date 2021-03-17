using InvestmentManager.Services.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Implimentations
{
    public class WebService : IWebService
    {
        private readonly HttpClient httpClient;
        public WebService(HttpClient httpClient) => this.httpClient = httpClient;

        public async Task<HttpResponseMessage> GetDataAsync(string query) => await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, query));
        public async Task<HttpResponseMessage> GetCBRateAsync() => await GetDataAsync("https://www.cbr-xml-daily.ru/daily_json.js");
    }
}
