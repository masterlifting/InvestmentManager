using Blazored.LocalStorage;
using InvestmentManager.Client.Configurations;
using InvestmentManager.Client.Services.NotificationService;
using InvestmentManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.HttpService
{
    public class APIService<T> where T : class
    {
        private readonly ILocalStorageService localStorage;
        private readonly CustomHttpClient http;
        private readonly CustomNotification notice;

        private static Func<ClaimsPrincipal, string> AccountIdBuilder => (ClaimsPrincipal user) => $"{user.Identity.Name}_{DefaultString.Id.accountId}";

        public APIService(ILocalStorageService localStorage, CustomHttpClient http, CustomNotification notice)
        {
            this.localStorage = localStorage;
            this.http = http;
            this.notice = notice;
        }
        public async Task<(BaseListViewModel<T> ViewResult, List<ColumnConfig> Columns)> GetResultsAsync(ClaimsPrincipal user, Func<long, string> urlBuilder, Func<ColumnConfig[]> columnBuilder = null)
        {
            List<T> items = null;
            string resultInfo = null;
            List<ColumnConfig> columns = null;

            if (user.Identity.IsAuthenticated)
            {
                if (await localStorage.ContainKeyAsync(AccountIdBuilder.Invoke(user)))
                {
                    var accountIds = await localStorage.GetItemAsync<long[]>(AccountIdBuilder.Invoke(user));

                    if (accountIds.Any())
                    {
                        var previewResults = new List<T>();

                        foreach (var accountId in accountIds)
                        {
                            string uri = urlBuilder.Invoke(accountId);
                            var previewResult = await http.GetAsync<List<T>>(uri);

                            if (previewResult != default)
                                previewResults.AddRange(previewResult);
                        }

                        if (previewResults.Any())
                        {
                            items = previewResults;

                            if (columnBuilder is not null)
                            {
                                columns = new List<ColumnConfig>();
                                columns.AddRange(columnBuilder.Invoke());
                            }
                        }
                        else
                            resultInfo = DefaultString.notFound;
                    }
                    else
                        resultInfo = DefaultString.accountDisabled;
                }
                else
                    resultInfo = DefaultString.accountNotFound;
            }
            else
                resultInfo = DefaultString.noticeAccess;

            return (new BaseListViewModel<T> { ResultInfo = resultInfo, ResultContents = items }, columns);
        }
        public async Task<(BaseListViewModel<T> ViewResult, List<ColumnConfig> Columns)> GetResultsAsync(Func<string> urlBuilder, Func<ColumnConfig[]> columnBuilder = null)
        {
            string resultInfo = null;
            List<ColumnConfig> columns = null;

            string uri = urlBuilder.Invoke();
            List<T> items = await http.GetAsync<List<T>>(uri);

            if (items != default && items.Any())
            {
                if (columnBuilder is not null)
                {
                    columns = new List<ColumnConfig>();
                    columns.AddRange(columnBuilder.Invoke());
                }
            }
            else
                resultInfo = DefaultString.notFound;

            return (new BaseListViewModel<T> { ResultContents = items, ResultInfo = resultInfo }, columns);
        }
        public async Task<BaseViewModel<T>> GetResultAsync(ClaimsPrincipal user, Func<long, string> urlBuilder, Func<List<T>, T> resultBuilder)
        {
            T item = null;
            string resultInfo = null;

            if (user.Identity.IsAuthenticated)
            {
                if (await localStorage.ContainKeyAsync(AccountIdBuilder.Invoke(user)))
                {
                    var accountIds = await localStorage.GetItemAsync<long[]>(AccountIdBuilder.Invoke(user));

                    if (accountIds.Any())
                    {
                        var previewResults = new List<T>();

                        foreach (var accountId in accountIds)
                        {
                            string uri = urlBuilder.Invoke(accountId);
                            var previewResult = await http.GetAsync<T>(uri);

                            if (previewResult != default)
                                previewResults.Add(previewResult);
                        }

                        if (previewResults.Any())
                            item = resultBuilder.Invoke(previewResults);
                        else
                            resultInfo = DefaultString.notFound;
                    }
                    else
                        resultInfo = DefaultString.accountDisabled;
                }
                else
                    resultInfo = DefaultString.accountNotFound;
            }
            else
                resultInfo = DefaultString.noticeAccess;

            return new BaseViewModel<T> { ResultContent = item, ResultInfo = resultInfo };
        }
        public async Task<BaseViewModel<T>> GetResultAsync(Func<string> urlBuilder)
        {
            T item = default;
            string resultInfo = null;

            string uri = urlBuilder.Invoke();
            var previewResult = await http.GetAsync<T>(uri);

            if (previewResult != default)
                item = previewResult;
            else
                resultInfo = DefaultString.notFound;

            return new BaseViewModel<T> { ResultContent = item, ResultInfo = resultInfo };
        }
        public async Task<BaseViewModel<T>> GetResultAsync(ClaimsPrincipal user, Func<string> urlBuilder)
        {
            T item = default;
            string resultInfo = null;

            if (user.Identity.IsAuthenticated)
            {
                string uri = urlBuilder.Invoke();
                var previewResult = await http.GetAsync<T>(uri);

                if (previewResult != default)
                    item = previewResult;
                else
                    resultInfo = DefaultString.notFound;
            }
            else
                resultInfo = DefaultString.noticeAccess;

            return new BaseViewModel<T> { ResultInfo = resultInfo, ResultContent = item };
        }
        public async Task PostDataAsync(T model, List<T> items, string controllerName, string name = null)
        {
            var result = await http.PostAsync<BaseActionResult, T>(controllerName, model);

            if (result.IsSuccess)
            {
                await notice.AlertSuccesAsync(result.Info);
                items.Remove(model);
            }
            else
                notice.ToastDanger($"{name} error.", result.Info);
        }
        public async Task PostDataCollectionAsync(List<T> items, string controllerName, string name = null)
        {
            if (items is not null && items.Any())
            {
                int errorCount = 0;

                for (int i = 0; i < items.Count; i++)
                {
                    var result = await http.PostAsync<BaseActionResult, T>(controllerName, items[i]);

                    if (result.IsSuccess)
                    {
                        items.RemoveAt(i);
                        i--;
                    }
                    else
                        errorCount++;
                }

                if (errorCount == 0)
                    await notice.AlertSuccesAsync($"All {name} saved");
                else
                    notice.ToastDanger($"Not saved {name} count:", errorCount.ToString());
            }
        }
    }
}
