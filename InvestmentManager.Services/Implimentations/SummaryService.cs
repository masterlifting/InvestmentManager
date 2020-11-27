using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Implimentations
{
    public class SummaryService : ISummaryService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public SummaryService(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;
        
        public async Task<decimal> GetAccountSumAsync(long accountId, decimal dollar)
        {
            #region Загрузка данных из БД
            var companyIds = await unitOfWork.Company.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);
            var dividends = unitOfWork.Dividend.GetAll().Where(x => accountId == x.AccountId);
            var comissions = unitOfWork.Comission.GetAll().Where(x => accountId == x.AccountId);
            var accountTransactions = unitOfWork.AccountTransaction.GetAll().Where(x => accountId == x.AccountId);
            var stockTransactions = (await unitOfWork.Ticker.GetTikersIncludeTransactions(accountId).ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.CompanyId);
            var exchangeRates = unitOfWork.ExchangeRate.GetAll().Where(x => accountId == x.AccountId);
            var lastPrices = unitOfWork.Price.GetLastPrices(30);
            #endregion

            #region Что нужно расчитать...
            decimal cleanInvestSumRub = 0; // Чистая сумма инвестиций (рублевая часть)
            decimal cleanInvestSumUsd = 0;// Чистая сумма инвестиций (валютная часть)
            decimal circulationActualSumRub = 0; // Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (рублевая часть)
            decimal circulationActualSumUsd = 0;// Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (валютная часть)
            decimal circulationOriginalSumRub = 0; // Стоимость акций, которые сейчас не проданы, на момент их покупки (рублевая часть)
            decimal circulationOriginalSumUsd = 0;// Стоимость акций, которые сейчас не проданы, на момент их покупки (валютная часть)
            decimal fixedProfitRub = 0; // Зафиксированная прибыль по акциям, которых больше нет в портфеле (рублевая часть)
            decimal fixedProfitUsd = 0;// Зафиксированная прибыль по акциям, которых больше нет в портфеле (валютная часть)
            decimal freeSumRub = 0;// Свободных средств в портфеле (рублевая часть)
            decimal freeSumUsd = 0;// Свободных средств в портфеле (валютная часть)
            decimal summarySumRub = 0;// Полная стоимость портфеля (рублевая часть)
            decimal summarySumUsd = 0;// Полная стоимость портфеля (валютная часть)
            #endregion

            #region Комиссии
            decimal comissionSum = 0;
            if (comissions != null)
                comissionSum = await comissions.SumAsync(x => x.Amount).ConfigureAwait(false);
            #endregion
            #region Дивиденды
            decimal dividendSumUsd = 0;
            decimal dividendSumRub = 0;
            if (dividends != null)
            {
                dividendSumUsd = dividends.Where(x => x.CurrencyId == 1).Sum(x => x.Amount);
                dividendSumRub = dividends.Where(x => x.CurrencyId == 2).Sum(x => x.Amount);
            }
            #endregion
            #region Операции по счетам
            decimal investSumUsd = 0;
            decimal investSumRub = 0;
            decimal withdrowSumUsd = 0;
            decimal withdrowSumRub = 0;
            if (accountTransactions != null)
            {
                var investmentSum = accountTransactions.Where(x => x.TransactionStatusId == 1);
                var withdrowSum = accountTransactions.Where(x => x.TransactionStatusId == 2);
                if (investmentSum != null)
                {
                    investSumUsd = await investmentSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
                    investSumRub = await investmentSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
                }
                if (withdrowSum != null)
                {
                    withdrowSumUsd = await withdrowSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
                    withdrowSumRub = await withdrowSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
                }
            }
            #endregion
            #region Обмены валют
            decimal usdSumBuy = 0;
            decimal usdSumSell = 0;
            decimal usdQuantityBuy = 0;
            decimal usdQuantitySell = 0;
            if (exchangeRates != null)
            {
                var usdBuy = exchangeRates.Where(x => x.TransactionStatusId == 3);
                var usdSell = exchangeRates.Where(x => x.TransactionStatusId == 4);
                if (usdBuy != null)
                {
                    usdSumBuy = await usdBuy.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
                    usdQuantityBuy = await usdBuy.SumAsync(x => x.Quantity).ConfigureAwait(false);
                }
                if (usdSell != null)
                {
                    usdSumSell = await usdSell.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
                    usdQuantitySell = await usdSell.SumAsync(x => x.Quantity).ConfigureAwait(false);
                }
            }
            #endregion
            #region Пересчет операций по портфелю с учетом обмена валют
            investSumRub -= usdSumBuy;
            investSumUsd += usdQuantityBuy;
            investSumUsd -= usdQuantitySell;
            investSumRub += usdSumSell;

            if (investSumRub != 0)
                cleanInvestSumRub = investSumRub - withdrowSumRub;
            if (investSumUsd != 0)
                cleanInvestSumUsd = investSumUsd - withdrowSumUsd;
            #endregion
            #region Расчет транзакций с акциями по разном направлениям
            foreach (var i in companyIds.Join(stockTransactions, x => x, y => y.Key, (x, y) => new
            {
                CompanyId = x,
                StockTransactions = y.Select(x => x.StockTransactions.Where(y => y.AccountId == accountId)).Aggregate((x, y) => x.Union(y))
            }))
            {
                var buyTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 3);
                var sellTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 4);

                decimal buyQuantity = buyTransactions.Sum(x => x.Quantity);
                decimal sellQuantity = sellTransactions.Sum(x => x.Quantity);

                decimal fixedSum = 0;
                decimal originalSum = 0;
                decimal actualSum = 0;
                // - Если проданы все акции этой компании
                if (buyQuantity == sellQuantity && buyQuantity != 0)
                {
                    decimal buySum = buyTransactions.Sum(x => x.Quantity * x.Cost);
                    decimal sellSum = sellTransactions.Sum(x => x.Quantity * x.Cost);
                    fixedSum = sellSum - buySum;

                }
                // - Если еще есть непроданные акции в портфеле
                else if (buyQuantity > sellQuantity)
                {
                    bool isLastPrice = lastPrices.TryGetValue(i.CompanyId, out decimal lastPrice);
                    if (!isLastPrice)
                        continue;

                    actualSum = (buyQuantity - sellQuantity) * lastPrice;

                    foreach (var transaction in i.StockTransactions.OrderBy(x => x.DateOperation))
                    {
                        if (transaction.TransactionStatusId == 3)
                            originalSum += (transaction.Quantity * transaction.Cost);
                        else if (transaction.TransactionStatusId == 4)
                            originalSum -= (transaction.Quantity * transaction.Cost);
                    }
                }
                // - расклад по валютам
                if (i.StockTransactions.First().CurrencyId == 1)
                {
                    fixedProfitUsd += fixedSum;
                    circulationOriginalSumUsd += originalSum;
                    circulationActualSumUsd += actualSum;
                }
                else
                {
                    fixedProfitRub += fixedSum;
                    circulationOriginalSumRub += originalSum;
                    circulationActualSumRub += actualSum;
                }
            }
            #endregion

            if (cleanInvestSumRub != 0)
                freeSumRub = cleanInvestSumRub - circulationOriginalSumRub + fixedProfitRub + dividendSumRub - comissionSum;
            if (cleanInvestSumUsd != 0)
                freeSumUsd = cleanInvestSumUsd - circulationOriginalSumUsd + fixedProfitUsd + dividendSumUsd;

            summarySumRub = circulationActualSumRub + freeSumRub;
            summarySumUsd = circulationActualSumUsd + freeSumUsd;


            return summarySumRub + summarySumUsd * dollar;
        }
    }
}
