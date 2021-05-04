using InvestmentManager.Entities.Broker;
using System.Collections.Generic;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Interfaces
{
    public interface ISummaryService
    {
        Task<decimal> GetAccountInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetAccountSumAsync(long accountId);
        Task<decimal> GetAccountSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesActualInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesFixedProfitSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesOriginalInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompanyActualInvestedSumAsync(long companyId);

        Task<bool> SetAccountFreeSumAsync(long accountId, long currencyId);
        Task<bool> SetAccountSummaryAsync(AccountTransaction transaction);
        Task<bool> SetAccountSummaryAsync(IEnumerable<AccountTransaction> transactions);
        Task<bool> SetCompanySummaryAsync(StockTransaction transaction);
        Task<bool> SetCompanySummaryAsync(IEnumerable<StockTransaction> transactions);
        Task<bool> SetDividendSummaryAsync(Dividend dividend);
        Task<bool> SetDividendSummaryAsync(IEnumerable<Dividend> dividends);
        Task<bool> SetComissionSummaryAsync(Comission comission);
        Task<bool> SetComissionSummaryAsync(IEnumerable<Comission> comissions);
        Task<bool> SetAccountSummaryAsync(ExchangeRate exchangeRate);
        Task<bool> SetAccountSummaryAsync(IEnumerable<ExchangeRate> exchangeRates);
        Task<bool> SetExchangeRateSummaryAsync(ExchangeRate exchangeRate);
        Task<bool> SetExchangeRateSummaryAsync(IEnumerable<ExchangeRate> exchangeRates);

        Task<bool> ResetSummaryDataAsync(string userId);
        Task<bool> ResetSummaryDataAsync(DataBaseType dbType, string[] userIds);
    }
}
