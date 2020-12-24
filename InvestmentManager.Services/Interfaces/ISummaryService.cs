using InvestmentManager.Entities.Broker;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Interfaces
{
    public interface ISummaryService
    {
        Task<decimal> GetAccountInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetAccountTotalSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesActualInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesFixedProfitSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompaniesOriginalInvestedSumAsync(long accountId, long currencyId);
        Task<decimal> GetCompanyActualInvestedSumAsync(long companyId);

        Task SetAccountFreeSumAsync(long accountId, long currencyId);
        Task SetAccountSummaryAsync(AccountTransaction transaction);
        Task SetAccountSummaryAsync(ExchangeRate exchangeRate);
        Task SetComissionSummaryAsync(Comission comission);
        Task SetCompanySummaryAsync(StockTransaction transaction);
        Task SetDividendSummaryAsync(Dividend dividend);
        Task SetExchangeRateSummaryAsync(ExchangeRate exchangeRate);

        Task ResetAllSummaryDataAsync(string userId);
    }
}
