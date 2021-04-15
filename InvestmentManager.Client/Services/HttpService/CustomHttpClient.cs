using InvestmentManager.Client.Services.AuthenticationConfiguration;
using InvestmentManager.Client.Services.NotificationService;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class CustomHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly CustomNotification notice;
        private readonly CustomAuthenticationStateProvider customAuthenticationState;

        public CustomHttpClient(
            HttpClient httpClient
            , CustomNotification notice
            , CustomAuthenticationStateProvider customAuthenticationState)
        {
            this.notice = notice;
            this.httpClient = httpClient;
            this.customAuthenticationState = customAuthenticationState;
        }

        public async Task GetAsync(string url)
        {
            await SetAuthHeaderAsync();
            notice.ToastWarning("Longer process.", "This request do not have a response!");
            await httpClient.GetAsync(url);
        }

        public async Task<TResult> GetAsync<TResult>(string url) => await BaseQueryAsync<TResult>(httpClient.GetAsync(url));
        public async Task<TResult> PostAsync<TResult, TModel>(string url, TModel model) => await BaseQueryAsync<TResult>(httpClient.PostAsJsonAsync(url, model));
        public async Task<TResult> PostContentAsync<TResult>(string url, HttpContent content) => await BaseQueryAsync<TResult>(httpClient.PostAsync(url, content));
        public async Task<TResult> PutAsync<TResult, TModel>(string url, TModel model) => await BaseQueryAsync<TResult>(httpClient.PutAsJsonAsync(url, model));
        public async Task<TResult> DeleteAsync<TResult>(string url) => await BaseQueryAsync<TResult>(httpClient.DeleteAsync(url));

        async Task<TResult> BaseQueryAsync<TResult>(Task<HttpResponseMessage> responseTask)
        {
            TResult result;
            notice.LoadStart();

            await SetAuthHeaderAsync();
            var response = await responseTask;

            if (response.IsSuccessStatusCode)
            {
                result = response.StatusCode == System.Net.HttpStatusCode.NoContent
                    ? default
                    : await response.Content.ReadFromJsonAsync<TResult>();
            }
            else
            {
                result = response.Content is not null
                    ? await response.Content.ReadFromJsonAsync<TResult>()
                    : Activator.CreateInstance<TResult>();
            }

            notice.LoadStop();
            return result;
        }
        public async Task SetAuthHeaderAsync()
        {
            string token = await customAuthenticationState.GetTokenAsync();
            httpClient.DefaultRequestHeaders.Authorization = token != null ? new AuthenticationHeaderValue("Bearer", token) : null;
        }
    }
}
