using InvestmentManager.Calculator;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Implimentations
{
    public class ReckonerService : IReckonerService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ISummaryService summaryService;
        private readonly IInvestCalculator calculator;

        public ReckonerService(IUnitOfWorkFactory unitOfWork
            , ISummaryService summaryService
            , IInvestCalculator calculator)
        {
            this.unitOfWork = unitOfWork;
            this.summaryService = summaryService;
            this.calculator = calculator;
        }

        public async Task UpgradeByAccountTransactionChangeAsync(AccountTransaction transaction)
        {
            if (await summaryService.SetAccountSummaryAsync(transaction).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);

        }
        public async Task UpgradeByStockTransactionChangeAsync(StockTransaction transaction, string userId)
        {
            if (await summaryService.SetCompanySummaryAsync(transaction).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(transaction.AccountId, transaction.CurrencyId).ConfigureAwait(false)
            && await calculator.SetSellRecommendationsAsync(userId).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task UpgradeByComissionChangeAsync(Comission comission)
        {
            if (await summaryService.SetComissionSummaryAsync(comission).ConfigureAwait(false)
                && await summaryService.SetAccountFreeSumAsync(comission.AccountId, comission.CurrencyId).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task UpgradeByDividendChangeAsync(Dividend dividend)
        {
            if (await summaryService.SetDividendSummaryAsync(dividend).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(dividend.AccountId, dividend.CurrencyId).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);

        }
        public async Task UpgradeByExchangeRateChangeAsync(ExchangeRate exchangeRate)
        {
            if (await summaryService.SetExchangeRateSummaryAsync(exchangeRate).ConfigureAwait(false)
            && await summaryService.SetAccountSummaryAsync(exchangeRate).ConfigureAwait(false)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, exchangeRate.CurrencyId)
            && await summaryService.SetAccountFreeSumAsync(exchangeRate.AccountId, (long)CurrencyTypes.RUB))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }

        public async Task UpgradeByPriceChangeAsync(DataBaseType dbType, string[] userIds)
        {
            if (await calculator.SetRatingByPricesAsync().ConfigureAwait(false)
            && await calculator.SetBuyRecommendationsAsync(dbType).ConfigureAwait(false)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task UpgradeByReportChangeAsync(DataBaseType dbType, long companyId, string[] userIds)
        {
            if (await calculator.SetRatingByReportsAsync(companyId).ConfigureAwait(false)
            && await calculator.SetBuyRecommendationsAsync(dbType).ConfigureAwait(false)
            && await calculator.SetSellRecommendationsAsync(dbType, userIds).ConfigureAwait(false))
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
    }
}
