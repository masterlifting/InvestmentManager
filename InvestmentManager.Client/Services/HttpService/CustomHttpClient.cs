using InvestmentManager.Client.Services.NotificationService;
using Microsoft.AspNetCore.Components;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class CustomHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly Notification notice;

        public CustomHttpClient(
            HttpClient httpClient,
            Notification notification
            )
        {
            this.httpClient = httpClient;
            this.notice = notification;
        }

        #region Get
        async Task<TResult> BaseGetAsync<TResult>(string uri, bool withLoading) where TResult : class
        {
            TResult result;

            if (withLoading)
                notice.LoadStart();

            var response = await httpClient.GetAsync(uri).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadFromJsonAsync<TResult>().ConfigureAwait(false);
            else
            {
                await notice.AlertFailedAsync($"Sorry, something went wrong. Try relogin").ConfigureAwait(false);
                result = Activator.CreateInstance<TResult>();
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
        public async Task<bool> PostBoolAsync<TModel>(string route, TModel model, bool withConfirm = false)
        {
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
        public async Task<HttpResponseMessage> PostModelAsync<TModel>(string route, TModel model, bool withConfirm = false)
        {
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

            return response;
        }
        public async Task<HttpResponseMessage> PostContentAsync(string route, HttpContent content, bool withLoading = false, bool withConfirm = false)
        {
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
    }
}
