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
            await summaryService.SetAccountSummaryAsync(transaction).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId).ConfigureAwait(false);

        public async Task<bool> UpgradeByStockTransactionChangeAsync(StockTransaction transaction, string userId) =>
            await summaryService.SetCompanySummaryAsync(transaction).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId).ConfigureAwait(false)
            && await calculator.SetSellRecommendationAsync(userId, transaction.TickerId).ConfigureAwait(false);

        public async Task<bool> UpgradeByComissionChangeAsync(Comission comission) =>
            await summaryService.SetComissionSummaryAsync(comission).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(comission.AccountId, comission.CurrencyId).ConfigureAwait(false);

        public async Task<bool> UpgradeByDividendChangeAsync(Dividend dividend) =>
            await summaryService.SetDividendSummaryAsync(dividend).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(dividend.AccountId, dividend.CurrencyId).ConfigureAwait(false);

        public async Task<bool> UpgradeByExchangeRateChangeAsync(ExchangeRate exchangeRate) =>
            await summaryService.SetExchangeRateSummaryAsync(exchangeRate).ConfigureAwait(false)
            && await summaryService.SetAccountSummaryAsync(exchangeRate).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, exchangeRate.CurrencyId)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, (long)CurrencyTypes.rub);

        public async Task<bool> UpgradeByPriceChangeAsync(DataBaseType dbType, string[] userIds) =>
            await calculator.SetRatingByPricesAsync().ConfigureAwait(false)
            && await calculator.SetBuyRecommendationsAsync(dbType).ConfigureAwait(false)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds).ConfigureAwait(false);

        public async Task<bool> UpgradeByReportChangeAsync(DataBaseType dbType, long companyId, string[] userIds) =>
            await calculator.SetRatingByReportsAsync(companyId).ConfigureAwait(false)
            && await calculator.SetBuyRecommendationsAsync(dbType).ConfigureAwait(false)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds).ConfigureAwait(false);
    }
}
