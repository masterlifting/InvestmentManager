using InvestmentManager.Client.Services.NotificationService;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using System;

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

        public async Task<TResult> GetResultAsync<TResult>(string route, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>(route).ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, long id, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?id={id}").ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
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
        public async Task<TResult> GetResultAsync<TResult>(string route, int value, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?value={value}").ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, long id, int value, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?id={id}&value={value}").ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, long id, string values, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?id={id}&values={values}").ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, string values, bool withLoading = false)
        {
            if (withLoading)
                notice.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?values={values}").ConfigureAwait(false);

            if (withLoading)
                notice.LoadStop();

            return result;
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
    }
}
