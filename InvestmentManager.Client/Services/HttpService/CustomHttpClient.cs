using InvestmentManager.Client.Services.NotificationService;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class CustomHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly Notification notification;

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
            if (withLoading)
                notification.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>(route).ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, long id, bool withLoading = false)
        {
            if (withLoading)
                notification.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?id={id}").ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<TResult> GetSizeAsync<TResult>(string route, int size, bool withLoading = false)
        {
            if (withLoading)
                notification.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?size={size}").ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<TResult> GetResultAsync<TResult>(string route, string ids, bool withLoading = false)
        {
            if (withLoading)
                notification.LoadStart();

            var result = await httpClient.GetFromJsonAsync<TResult>($"{route}?ids={ids}").ConfigureAwait(false);

            if (withLoading)
                notification.LoadStop();

            return result;
        }
        public async Task<bool> GetBoolAsync(string route, long id, bool withConfirm = false)
        {
            notification.LoadStart();
            var response = await httpClient.GetAsync($"{route}?id={id}").ConfigureAwait(false);
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
            notification.LoadStart();
            var response = await httpClient.PostAsJsonAsync(route, model).ConfigureAwait(false);
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
            notification.LoadStart();
            var response = await httpClient.PostAsJsonAsync(route, model).ConfigureAwait(false);
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
            if (withLoading)
                notification.LoadStart();
            var response = await httpClient.PostAsync(route, content).ConfigureAwait(false);
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
