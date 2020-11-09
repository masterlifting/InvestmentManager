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

        #region Get
        async Task<TResult> BaseGetAsync<TResult>(string uri, bool withLoading) where TResult : class
        {
            TResult result;
            await SetAuthHeaderAsync().ConfigureAwait(false);

            if (withLoading)
                notice.LoadStart();

            var response = await httpClient.GetAsync(uri).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
            else
            {
                await notice.AlertFailedAsync($"Sorry, something went wrong.").ConfigureAwait(false);
                result = null;
            }

            if (withLoading)
                notice.LoadStop();

            return result;
        }

        public async Task<TResult> GetNoParametersAsync<TResult>(string route, bool withLoading = false) where TResult : class
        {
            return await BaseGetAsync<TResult>(route, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByIdAsync<TResult>(string route, long id, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?id={id}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByIdAsync<TResult>(string route, long? id, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?id={id}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByValueAsync<TResult>(string route, int value, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?value={value}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByIdValueAsync<TResult>(string route, long id, int value, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?id={id}&value={value}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByValuesAsync<TResult>(string route, string values, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?values={values}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }
        public async Task<TResult> GetByIdValuesAsync<TResult>(string route, long id, string values, bool withLoading = false) where TResult : class
        {
            string uri = $"{route}?id={id}&values={values}";
            return await BaseGetAsync<TResult>(uri, withLoading).ConfigureAwait(false);
        }

        public async Task<bool> GetBoolAsync(string route, long id, bool withConfirm = false)
        {
            await SetAuthHeaderAsync().ConfigureAwait(false);
            notice.LoadStart();
            var response = await httpClient.GetAsync($"{route}?id={id}").ConfigureAwait(false);
            notice.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notice.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notice.AlertFailedAsync().ConfigureAwait(false);
            }

            return response.IsSuccessStatusCode;
        }
        public async Task GetVoidAsync(string route, bool withConfirm = false)
        {
            await SetAuthHeaderAsync().ConfigureAwait(false);
            notice.LoadStart();
            var result = await httpClient.GetAsync(route).ConfigureAwait(false);
            notice.LoadStop();

            if (withConfirm)
            {
                if (result.IsSuccessStatusCode)
                    await notice.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notice.AlertFailedAsync().ConfigureAwait(false);
            }
        }
        #endregion
        #region Post
        public async Task<bool> PostAsBoolAsync<TModel>(string route, TModel model, bool withConfirm = false)
        {
            await SetAuthHeaderAsync().ConfigureAwait(false);
            notice.LoadStart();
            var response = await httpClient.PostAsJsonAsync(route, model).ConfigureAwait(false);
            notice.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notice.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notice.AlertFailedAsync().ConfigureAwait(false);
            }

            return response.IsSuccessStatusCode;
        }
        public async Task<TResult> PostAsModelAsync<TResult, TModel>(string route, TModel model, bool withConfirm = false)
        {
            await SetAuthHeaderAsync().ConfigureAwait(false);
            notice.LoadStart();
            var response = await httpClient.PostAsJsonAsync(route, model).ConfigureAwait(false);
            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notice.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notice.AlertFailedAsync().ConfigureAwait(false);
            }
            notice.LoadStop();

            return await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
        }
        public async Task<HttpResponseMessage> PostAsContentAsync(string route, HttpContent content, bool withLoading = false, bool withConfirm = false)
        {
            await SetAuthHeaderAsync().ConfigureAwait(false);
            if (withLoading)
                notice.LoadStart();
            var response = await httpClient.PostAsync(route, content).ConfigureAwait(false);
            if (withLoading)
                notice.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notice.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notice.AlertFailedAsync().ConfigureAwait(false);
            }

            return response;
        }
        #endregion
        public async Task SetAuthHeaderAsync()
        {
            string token = await customAuthenticationState.GetTokenAsync().ConfigureAwait(false);
            if (token != null)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        public void SetDefaultAuthHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
