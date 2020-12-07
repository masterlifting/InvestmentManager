using Blazored.LocalStorage;
using InvestmentManager.Client.Configurations;
using InvestmentManager.Client.Services.HttpService;
using InvestmentManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InvestmentManager.Client.Services.QueryService
{
    public class QueryBaseService<T> where T : class
    {
        private readonly ILocalStorageService localStorage;
        private readonly CustomHttpClient http;
        private Func<ClaimsPrincipal, string> accountIdBuilder => (ClaimsPrincipal user) => $"{user.Identity.Name}_{DefaultString.Id.accountId}";

        public QueryBaseService(ILocalStorageService localStorage, CustomHttpClient http)
        {
            this.localStorage = localStorage;
            this.http = http;
        }
        public async Task<(BaseListViewModel<T> ViewResult, List<ColumnConfig> Columns)> GetResultsAsync(ClaimsPrincipal user, Func<long, string> urlBuilder, Func<ColumnConfig[]> columnBuilder = null)
        {
            List<T> items = null;
            string resultInfo = null;
            List<ColumnConfig> columns = null;

            if (user.Identity.IsAuthenticated)
            {
                if (await localStorage.ContainKeyAsync(accountIdBuilder.Invoke(user)))
                {
                    var accountIds = await localStorage.GetItemAsync<long[]>(accountIdBuilder.Invoke(user));

                    if (accountIds.Any())
                    {
                        var previewResults = new List<T>();

                        foreach (var accountId in accountIds)
                        {
                            string uri = urlBuilder.Invoke(accountId);
                            var previewResult = await http.GetAsync<List<T>>(uri).ConfigureAwait(false);
                            
                            if(previewResult != default)
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
            List<T> items = await http.GetAsync<List<T>>(uri).ConfigureAwait(false);

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
                if (await localStorage.ContainKeyAsync(accountIdBuilder.Invoke(user)))
                {
                    var accountIds = await localStorage.GetItemAsync<long[]>(accountIdBuilder.Invoke(user));

                    if (accountIds.Any())
                    {
                        var previewResults = new List<T>();

                        foreach (var accountId in accountIds)
                        {
                            string uri = urlBuilder.Invoke(accountId);
                            var previewResult = await http.GetAsync<T>(uri).ConfigureAwait(false);
                            
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

            return new BaseViewModel<T> { ResultContent = item, ResultInfo = resultInfo};
        }
        public async Task<BaseViewModel<T>> GetResultAsync(Func<string> urlBuilder)
        {
            T item = default;
            string resultInfo = null;

            string uri = urlBuilder.Invoke();
            var previewResult = await http.GetAsync<T>(uri).ConfigureAwait(false);

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
                var previewResult = await http.GetAsync<T>(uri).ConfigureAwait(false);

                if (previewResult != default)
                    item = previewResult;
                else
                    resultInfo = DefaultString.notFound;
            }
            else
                resultInfo = DefaultString.noticeAccess;

            return new BaseViewModel<T> { ResultInfo = resultInfo, ResultContent = item };
        }
    }
}
