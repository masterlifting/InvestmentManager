using InvestmentManager.Client.Services.NotificationService;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class CustomHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly Notification notification;
        readonly int timeThreshold = 200;

        public CustomHttpClient(
            HttpClient httpClient,
            Notification notification
            )
        {
            this.httpClient = httpClient;
            this.notification = notification;
        }

        public async Task<TResult> GetResultAsync<TResult>(string route, bool withLoading = false)
        {
            var taskResult = httpClient.GetFromJsonAsync<TResult>(route);

            if (withLoading)
            {
                var startTime = DateTime.Now;
                if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResult.IsCompleted)
                    notification.LoadStart();
            }

            var result = await taskResult.ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, long id, bool withLoading = false)
        {
            var taskResult = httpClient.GetFromJsonAsync<TResult>($"{route}?id={id}");

            if (withLoading)
            {
                var startTime = DateTime.Now;
                if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResult.IsCompleted)
                    notification.LoadStart();
            }

            var result = await taskResult.ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<bool> GetBoolAsync(string route, long id, bool withConfirm = false)
        {
            var startTime = DateTime.Now;
            var taskResponse = httpClient.GetAsync($"{route}?id={id}");

            if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResponse.IsCompleted)
                notification.LoadStart();

            var response = await taskResponse.ConfigureAwait(false);
            notification.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notification.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notification.AlertFailedAsync().ConfigureAwait(false);
            }

            return response.IsSuccessStatusCode;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, string ids, bool withLoading = false)
        {
            var taskResult = httpClient.GetFromJsonAsync<TResult>($"{route}?ids={ids}");

            if (withLoading)
            {
                var startTime = DateTime.Now;
                if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResult.IsCompleted)
                    notification.LoadStart();
            }

            var result = await taskResult.ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }


        public async Task GetVoidAsync(string route, bool withConfirm = false)
        {
            notification.LoadStart();
            var result = await httpClient.GetAsync(route).ConfigureAwait(false);
            notification.LoadStop();

            if (withConfirm)
            {
                if (result.IsSuccessStatusCode)
                    await notification.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notification.AlertFailedAsync().ConfigureAwait(false);
            }
        }
        public async Task<bool> PostBoolAsync<TModel>(string route, TModel model, bool withConfirm = false)
        {
            var startTime = DateTime.Now;
            var taskResponse = httpClient.PostAsJsonAsync(route, model);
            if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResponse.IsCompleted)
                notification.LoadStart();

            var response = await taskResponse.ConfigureAwait(false);
            notification.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notification.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notification.AlertFailedAsync().ConfigureAwait(false);
            }

            return response.IsSuccessStatusCode;
        }
        public async Task<HttpResponseMessage> PostModelAsync<TModel>(string route, TModel model, bool withConfirm = false)
        {
            var startTime = DateTime.Now;
            var taskResponse = httpClient.PostAsJsonAsync(route, model);
            if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResponse.IsCompleted)
                notification.LoadStart();

            var response = await taskResponse.ConfigureAwait(false);
            notification.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notification.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notification.AlertFailedAsync().ConfigureAwait(false);
            }

            return response;
        }
        public async Task<HttpResponseMessage> PostContentAsync(string route, HttpContent content, bool withLoading = false, bool withConfirm = false)
        {
            var taskResponse = httpClient.PostAsync(route, content);

            if (withLoading)
            {
                var startTime = DateTime.Now;
                if ((DateTime.Now - startTime).TotalMilliseconds > timeThreshold && !taskResponse.IsCompleted)
                    notification.LoadStart();
            }

            var response = await taskResponse.ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            if (withConfirm)
            {
                if (response.IsSuccessStatusCode)
                    await notification.AlertSuccesAsync().ConfigureAwait(false);
                else
                    await notification.AlertFailedAsync().ConfigureAwait(false);
            }

            return response;
        }
    }
}
