using InvestmentManager.Entities.Broker;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

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

        Task<bool> SetAccountFreeSumAsync(long accountId, long currencyId);
        Task<bool> SetAccountSummaryAsync(AccountTransaction transaction);
        Task<bool> SetAccountSummaryAsync(ExchangeRate exchangeRate);
        Task<bool> SetComissionSummaryAsync(Comission comission);
        Task<bool> SetCompanySummaryAsync(StockTransaction transaction);
        Task<bool> SetDividendSummaryAsync(Dividend dividend);
        Task<bool> SetExchangeRateSummaryAsync(ExchangeRate exchangeRate);

        Task<bool> ResetSummaryDataAsync(string userId);
        Task<bool> ResetSummaryDataAsync(DataBaseType dbType, string[] userIds);
    }
}
