using InvestmentManager.Client.Configurations;
using InvestmentManager.Client.Services.AuthenticationConfiguration;
using InvestmentManager.Client.Services.NotificationService;
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
            await SetAuthHeaderAsync().ConfigureAwait(false);
            notice.ToastWarning("Longer process.", "This request do not have a response!");
            await httpClient.GetAsync(url).ConfigureAwait(false);
        }

        public async Task<TResult> GetAsync<TResult>(string url) => await BaseQueryAsync<TResult>(httpClient.GetAsync(url)).ConfigureAwait(false);
        public async Task<TResult> PostAsync<TResult, TModel>(string url, TModel model) => await BaseQueryAsync<TResult>(httpClient.PostAsJsonAsync(url, model)).ConfigureAwait(false);
        public async Task<TResult> PostContentAsync<TResult>(string url, HttpContent content) => await BaseQueryAsync<TResult>(httpClient.PostAsync(url, content)).ConfigureAwait(false);
        public async Task<TResult> PutAsync<TResult, TModel>(string url, TModel model) => await BaseQueryAsync<TResult>(httpClient.PutAsJsonAsync(url, model)).ConfigureAwait(false);
        public async Task<TResult> DeleteAsync<TResult>(string url) => await BaseQueryAsync<TResult>(httpClient.DeleteAsync(url)).ConfigureAwait(false);

        async Task<TResult> BaseQueryAsync<TResult>(Task<HttpResponseMessage> responseTask)
        {
            TResult result;
            notice.LoadStart();

            await SetAuthHeaderAsync().ConfigureAwait(false);
            var response = await responseTask.ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                result = response.StatusCode == System.Net.HttpStatusCode.NoContent
                    ? default
                    : await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
            }
            else
            {
                await notice.AlertFailedAsync(DefaultString.noticeFailed);
                result = await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
            }

            notice.LoadStop();
            return result;
        }
        public async Task SetAuthHeaderAsync()
        {
            string token = await customAuthenticationState.GetTokenAsync().ConfigureAwait(false);
            httpClient.DefaultRequestHeaders.Authorization = token != null ? new AuthenticationHeaderValue("Bearer", token) : null;
        }
    }
}
