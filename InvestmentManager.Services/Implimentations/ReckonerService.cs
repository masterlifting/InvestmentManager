using InvestmentManager.Calculator;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Services.Interfaces;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Implimentations
{
    public class ReckonerService : IReckonerService
    {
        private readonly ISummaryService summaryService;
        private readonly IInvestCalculator calculator;

        public ReckonerService(
            ISummaryService summaryService
            , IInvestCalculator calculator)
        {
            this.summaryService = summaryService;
            this.calculator = calculator;
        }

        public async Task<bool> UpgradeByAccountTransactionChangeAsync(AccountTransaction transaction) =>
            await summaryService.SetAccountSummaryAsync(transaction)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId);

        public async Task<bool> UpgradeByStockTransactionChangeAsync(StockTransaction transaction, string userId) =>
            await summaryService.SetCompanySummaryAsync(transaction)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId)
            && await calculator.SetSellRecommendationAsync(userId, transaction.TickerId);

        public async Task<bool> UpgradeByComissionChangeAsync(Comission comission) =>
            await summaryService.SetComissionSummaryAsync(comission)
            && await summaryService.SetAccountFreeSumAsync(comission.AccountId, comission.CurrencyId);

        public async Task<bool> UpgradeByDividendChangeAsync(Dividend dividend) =>
            await summaryService.SetDividendSummaryAsync(dividend)
            && await summaryService.SetAccountFreeSumAsync(dividend.AccountId, dividend.CurrencyId);

        public async Task<bool> UpgradeByExchangeRateChangeAsync(ExchangeRate exchangeRate) =>
            await summaryService.SetExchangeRateSummaryAsync(exchangeRate)
            && await summaryService.SetAccountSummaryAsync(exchangeRate)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, exchangeRate.CurrencyId)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, (long)CurrencyTypes.rub);

        public async Task<bool> UpgradeByPriceChangeAsync(DataBaseType dbType, string[] userIds) =>
            await calculator.SetRatingByPricesAsync()
            && await calculator.SetBuyRecommendationsAsync(dbType)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds);

        public async Task<bool> UpgradeByReportChangeAsync(DataBaseType dbType, long companyId, string[] userIds) =>
            await calculator.SetRatingByReportsAsync(companyId)
            && await calculator.SetBuyRecommendationsAsync(dbType)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds);
    }
}
