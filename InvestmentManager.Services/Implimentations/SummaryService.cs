using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Implimentations
{
    public class SummaryService : ISummaryService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public SummaryService(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        #region test
        //public async Task<decimal> GetAccountSumAsync(long accountId, decimal dollar)
        //{
        //    #region Загрузка данных из БД
        //    var companyIds = await unitOfWork.Company.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);
        //    var dividends = unitOfWork.Dividend.GetAll().Where(x => accountId == x.AccountId);
        //    var comissions = unitOfWork.Comission.GetAll().Where(x => accountId == x.AccountId);
        //    var accountTransactions = unitOfWork.AccountTransaction.GetAll().Where(x => accountId == x.AccountId);
        //    var stockTransactions = (await unitOfWork.Ticker.GetTikersIncludeTransactions(accountId).ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.CompanyId);
        //    var exchangeRates = unitOfWork.ExchangeRate.GetAll().Where(x => accountId == x.AccountId);
        //    var lastPrices = unitOfWork.Price.GetLastPrices(30);
        //    #endregion

        //    #region Что нужно расчитать...
        //    decimal cleanInvestSumRub = 0; // Чистая сумма инвестиций (рублевая часть)
        //    decimal cleanInvestSumUsd = 0;// Чистая сумма инвестиций (валютная часть)
        //    decimal circulationActualSumRub = 0; // Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (рублевая часть)
        //    decimal circulationActualSumUsd = 0;// Стоимость всех акций, которые сейчас не проданы, с учетом последней доступной цены (валютная часть)
        //    decimal circulationOriginalSumRub = 0; // Стоимость акций, которые сейчас не проданы, на момент их покупки (рублевая часть)
        //    decimal circulationOriginalSumUsd = 0;// Стоимость акций, которые сейчас не проданы, на момент их покупки (валютная часть)
        //    decimal fixedProfitRub = 0; // Зафиксированная прибыль по акциям, которых больше нет в портфеле (рублевая часть)
        //    decimal fixedProfitUsd = 0;// Зафиксированная прибыль по акциям, которых больше нет в портфеле (валютная часть)
        //    decimal freeSumRub = 0;// Свободных средств в портфеле (рублевая часть)
        //    decimal freeSumUsd = 0;// Свободных средств в портфеле (валютная часть)
        //    decimal summarySumRub = 0;// Полная стоимость портфеля (рублевая часть)
        //    decimal summarySumUsd = 0;// Полная стоимость портфеля (валютная часть)
        //    #endregion

        //    #region Комиссии
        //    decimal comissionSum = 0;
        //    if (comissions != null)
        //        comissionSum = await comissions.SumAsync(x => x.Amount).ConfigureAwait(false);
        //    #endregion
        //    #region Дивиденды
        //    decimal dividendSumUsd = 0;
        //    decimal dividendSumRub = 0;
        //    if (dividends != null)
        //    {
        //        dividendSumUsd = dividends.Where(x => x.CurrencyId == 1).Sum(x => x.Amount);
        //        dividendSumRub = dividends.Where(x => x.CurrencyId == 2).Sum(x => x.Amount);
        //    }
        //    #endregion
        //    #region Операции по счетам
        //    decimal investSumUsd = 0;
        //    decimal investSumRub = 0;
        //    decimal withdrowSumUsd = 0;
        //    decimal withdrowSumRub = 0;
        //    if (accountTransactions != null)
        //    {
        //        var investmentSum = accountTransactions.Where(x => x.TransactionStatusId == 1);
        //        var withdrowSum = accountTransactions.Where(x => x.TransactionStatusId == 2);
        //        if (investmentSum != null)
        //        {
        //            investSumUsd = await investmentSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
        //            investSumRub = await investmentSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
        //        }
        //        if (withdrowSum != null)
        //        {
        //            withdrowSumUsd = await withdrowSum.Where(x => x.CurrencyId == 1).SumAsync(x => x.Amount).ConfigureAwait(false);
        //            withdrowSumRub = await withdrowSum.Where(x => x.CurrencyId == 2).SumAsync(x => x.Amount).ConfigureAwait(false);
        //        }
        //    }
        //    #endregion
        //    #region Обмены валют
        //    decimal usdSumBuy = 0;
        //    decimal usdSumSell = 0;
        //    decimal usdQuantityBuy = 0;
        //    decimal usdQuantitySell = 0;
        //    if (exchangeRates != null)
        //    {
        //        var usdBuy = exchangeRates.Where(x => x.TransactionStatusId == 3);
        //        var usdSell = exchangeRates.Where(x => x.TransactionStatusId == 4);
        //        if (usdBuy != null)
        //        {
        //            usdSumBuy = await usdBuy.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
        //            usdQuantityBuy = await usdBuy.SumAsync(x => x.Quantity).ConfigureAwait(false);
        //        }
        //        if (usdSell != null)
        //        {
        //            usdSumSell = await usdSell.SumAsync(x => x.Quantity * x.Rate).ConfigureAwait(false);
        //            usdQuantitySell = await usdSell.SumAsync(x => x.Quantity).ConfigureAwait(false);
        //        }
        //    }
        //    #endregion
        //    #region Пересчет операций по портфелю с учетом обмена валют
        //    investSumRub -= usdSumBuy;
        //    investSumUsd += usdQuantityBuy;
        //    investSumUsd -= usdQuantitySell;
        //    investSumRub += usdSumSell;

        //    if (investSumRub != 0)
        //        cleanInvestSumRub = investSumRub - withdrowSumRub;
        //    if (investSumUsd != 0)
        //        cleanInvestSumUsd = investSumUsd - withdrowSumUsd;
        //    #endregion
        //    #region Расчет транзакций с акциями по разном направлениям
        //    foreach (var i in companyIds.Join(stockTransactions, x => x, y => y.Key, (x, y) => new
        //    {
        //        CompanyId = x,
        //        StockTransactions = y.Select(x => x.StockTransactions.Where(y => y.AccountId == accountId)).Aggregate((x, y) => x.Union(y))
        //    }))
        //    {
        //        var buyTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 3);
        //        var sellTransactions = i.StockTransactions.Where(x => x.TransactionStatusId == 4);

        //        decimal buyQuantity = buyTransactions.Sum(x => x.Quantity);
        //        decimal sellQuantity = sellTransactions.Sum(x => x.Quantity);

        //        decimal fixedSum = 0;
        //        decimal originalSum = 0;
        //        decimal actualSum = 0;
        //        // - Если проданы все акции этой компании
        //        if (buyQuantity == sellQuantity && buyQuantity != 0)
        //        {
        //            decimal buySum = buyTransactions.Sum(x => x.Quantity * x.Cost);
        //            decimal sellSum = sellTransactions.Sum(x => x.Quantity * x.Cost);
        //            fixedSum = sellSum - buySum;

        //        }
        //        // - Если еще есть непроданные акции в портфеле
        //        else if (buyQuantity > sellQuantity)
        //        {
        //            bool isLastPrice = lastPrices.TryGetValue(i.CompanyId, out decimal lastPrice);
        //            if (!isLastPrice)
        //                continue;

        //            actualSum = (buyQuantity - sellQuantity) * lastPrice;

        //            foreach (var transaction in i.StockTransactions.OrderBy(x => x.DateOperation))
        //            {
        //                if (transaction.TransactionStatusId == 3)
        //                    originalSum += (transaction.Quantity * transaction.Cost);
        //                else if (transaction.TransactionStatusId == 4)
        //                    originalSum -= (transaction.Quantity * transaction.Cost);
        //            }
        //        }
        //        // - расклад по валютам
        //        if (i.StockTransactions.First().CurrencyId == 1)
        //        {
        //            fixedProfitUsd += fixedSum;
        //            circulationOriginalSumUsd += originalSum;
        //            circulationActualSumUsd += actualSum;
        //        }
        //        else
        //        {
        //            fixedProfitRub += fixedSum;
        //            circulationOriginalSumRub += originalSum;
        //            circulationActualSumRub += actualSum;
        //        }
        //    }
        //    #endregion

        //    if (cleanInvestSumRub != 0)
        //        freeSumRub = cleanInvestSumRub - circulationOriginalSumRub + fixedProfitRub + dividendSumRub - comissionSum;
        //    if (cleanInvestSumUsd != 0)
        //        freeSumUsd = cleanInvestSumUsd - circulationOriginalSumUsd + fixedProfitUsd + dividendSumUsd;

        //    summarySumRub = circulationActualSumRub + freeSumRub;
        //    summarySumUsd = circulationActualSumUsd + freeSumUsd;


        //    return summarySumRub + summarySumUsd * dollar;
        //}
        #endregion

        public async Task<decimal> GetAccountTotalSumAsync(long accountId, long currencyId)
        {
            var accountSummary = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId).ConfigureAwait(false);
            if (accountSummary is null)
                return 0;

            decimal companiesActualInvestedSum = await GetCompaniesActualInvestedSumAsync(accountId, currencyId).ConfigureAwait(false);

            return companiesActualInvestedSum + accountSummary.FreeSum;
        }
        public async Task<decimal> GetAccountInvestedSumAsync(long accountId, long currencyId)
        {
            var result = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId).ConfigureAwait(false);
            if (result is null)
                return 0;

            return result.InvestedSum;
        }

        public async Task<decimal> GetCompaniesFixedProfitSumAsync(long accountId, long currencyId) =>
            await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.CurrentProfit > 0).SumAsync(x => x.CurrentProfit).ConfigureAwait(false);
        public async Task<decimal> GetCompaniesOriginalInvestedSumAsync(long accountId, long currencyId) =>
            (await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.CurrentProfit < 0).SumAsync(x => x.CurrentProfit).ConfigureAwait(false)) * -1;
        public async Task<decimal> GetCompaniesActualInvestedSumAsync(long accountId, long currencyId)
        {
            var companySummaries = unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.ActualLot > 0);
            var actualPrices = unitOfWork.Price.GetLastPrices(30);
            return await companySummaries.Join(actualPrices, x => x.CompanyId, y => y.Key, (x, y) => x.ActualLot * y.Value).SumAsync().ConfigureAwait(false);
        }
        public async Task<decimal> GetCompanyActualInvestedSumAsync(long companyId)
        {
            var companySummary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.CompanyId == companyId).ConfigureAwait(false);
            if (companySummary.ActualLot == 0)
                return 0;

            var actualPrice = await unitOfWork.Price.GetCustomPricesAsync(companyId, 1, OrderType.OrderByDesc).ConfigureAwait(false);
            if (actualPrice is null)
                return 0;

            return companySummary.ActualLot * actualPrice.First().Value;
        }


        public async Task SetAccountSummaryAsync(AccountTransaction transaction)
        {
            if (transaction is null)
                throw new NullReferenceException("Transaction not found!");

            var summary = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == transaction.AccountId && x.CurrencyId == transaction.CurrencyId).ConfigureAwait(false);

            if (summary is not null)
            {
                summary.InvestedSum = transaction.TransactionStatusId switch
                {
                    1 => summary.InvestedSum + transaction.Amount,
                    2 => summary.InvestedSum - transaction.Amount,
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = transaction.AccountId,
                    CurrencyId = transaction.CurrencyId,
                    InvestedSum = transaction.Amount
                }).ConfigureAwait(false);
            }
        }
        public async Task SetAccountSummaryAsync(ExchangeRate exchangeRate)
        {
            if (exchangeRate is null)
                throw new NullReferenceException("Transaction not found!");

            var summaryIn = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId).ConfigureAwait(false);
            if (summaryIn is not null)
            {
                summaryIn.InvestedSum = exchangeRate.TransactionStatusId switch
                {
                    3 => summaryIn.InvestedSum + exchangeRate.Quantity,
                    4 => summaryIn.InvestedSum - exchangeRate.Quantity,
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = exchangeRate.AccountId,
                    CurrencyId = exchangeRate.CurrencyId,
                    InvestedSum = exchangeRate.Quantity
                }).ConfigureAwait(false);
            }

            var summaryOut = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == 2).ConfigureAwait(false);
            if (summaryOut is not null)
            {
                summaryOut.InvestedSum = exchangeRate.TransactionStatusId switch
                {
                    3 => summaryOut.InvestedSum - exchangeRate.Quantity * exchangeRate.Rate,
                    4 => summaryOut.InvestedSum + exchangeRate.Quantity * exchangeRate.Rate,
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = exchangeRate.AccountId,
                    CurrencyId = 2,
                    InvestedSum = exchangeRate.Quantity * exchangeRate.Rate
                }).ConfigureAwait(false);
            }
        }
        public async Task SetAccountFreeSumAsync(long accountId, long currencyId)
        {
            var accountSummary = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId).ConfigureAwait(false);

            if (accountSummary is null)
                throw new NullReferenceException("Account summary not found");

            var companySummary = await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId).ToListAsync().ConfigureAwait(false);
            var dividendSummary = unitOfWork.DividendSummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId);

            decimal companiesFixedProfitSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit > 0).Sum(x => x.CurrentProfit) : 0;
            decimal companiesOriginalInvestedSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit < 0).Sum(x => x.CurrentProfit) : 0;
            decimal companiesDividendSum = dividendSummary is not null ? await dividendSummary.SumAsync(x => x.TotalSum).ConfigureAwait(false) : 0;
            decimal companiesComissionSum = 0;
            if (currencyId == 2)
                companiesComissionSum = await unitOfWork.ComissionSummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId).SumAsync(x => x.TotalSum).ConfigureAwait(false);

            decimal result = accountSummary.InvestedSum - companiesOriginalInvestedSum + companiesFixedProfitSum + companiesDividendSum - companiesComissionSum;

            accountSummary.FreeSum = result;
        }

        public async Task SetCompanySummaryAsync(StockTransaction transaction)
        {
            if (transaction is null)
                throw new NullReferenceException("Transaction not found!");

            var companyId = (await unitOfWork.Ticker.FindByIdAsync(transaction.TickerId).ConfigureAwait(false))?.CompanyId;
            if (!companyId.HasValue)
                throw new NullReferenceException("Ticker not found!");

            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.CompanyId == companyId.Value).ConfigureAwait(false);

            if (summary is not null)
                switch(transaction.TransactionStatusId)
                {
                    case 3:
                        summary.ActualLot += transaction.Quantity;
                        summary.CurrentProfit -= (transaction.Quantity * transaction.Cost);
                        break;
                    case 4:
                        summary.ActualLot -= transaction.Quantity;
                        summary.CurrentProfit += (transaction.Quantity * transaction.Cost);
                        break;
                }
            else
                await unitOfWork.CompanySummary.CreateEntityAsync(new CompanySummary
                {
                    CompanyId = companyId.Value,
                    AccountId = transaction.AccountId,
                    CurrencyId = transaction.CurrencyId,
                    ActualLot = transaction.Quantity,
                    CurrentProfit = transaction.Quantity * transaction.Cost * -1
                }).ConfigureAwait(false);
        }
        public async Task SetDividendSummaryAsync(Dividend dividend)
        {
            if (dividend is null)
                throw new NullReferenceException("Dividend not found!");

            var companyId = (await unitOfWork.Isin.FindByIdAsync(dividend.IsinId).ConfigureAwait(false))?.CompanyId;
            if (!companyId.HasValue)
                throw new NullReferenceException("Isin not found!");

            var summary = await unitOfWork.DividendSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == dividend.AccountId && x.CurrencyId == dividend.CurrencyId && x.CompanyId == companyId.Value).ConfigureAwait(false);

            if (summary is not null)
            {
                summary.TotalSum += dividend.Amount;
                summary.TotalTax += dividend.Tax;
            }
            else
            {
                await unitOfWork.DividendSummary.CreateEntityAsync(new DividendSummary
                {
                    AccountId = dividend.AccountId,
                    CurrencyId = dividend.CurrencyId,
                    CompanyId = companyId.Value,
                    TotalSum = dividend.Amount,
                    TotalTax = dividend.Tax
                }).ConfigureAwait(false);
            }
        }
        public async Task SetComissionSummaryAsync(Comission comission)
        {
            if (comission is null)
                throw new NullReferenceException("Comission not found!");

            var summary = await unitOfWork.ComissionSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == comission.AccountId && x.CurrencyId == comission.CurrencyId).ConfigureAwait(false);

            if (summary is not null)
                summary.TotalSum += comission.Amount;
            else
            {
                await unitOfWork.ComissionSummary.CreateEntityAsync(new ComissionSummary
                {
                    AccountId = comission.AccountId,
                    CurrencyId = comission.CurrencyId,
                    TotalSum = comission.Amount
                }).ConfigureAwait(false);
            }
        }
        public async Task SetExchangeRateSummaryAsync(ExchangeRate exchangeRate)
        {
            if (exchangeRate is null)
                throw new NullReferenceException("Exchange rate not found!");

            var summary = await unitOfWork.ExchangeRateSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId).ConfigureAwait(false);

            var exchangeRateSummary = new ExchangeRateSummary
            {
                AccountId = exchangeRate.AccountId,
                CurrencyId = exchangeRate.CurrencyId
            };
            switch (exchangeRate.TransactionStatusId)
            {
                case 3:
                    exchangeRateSummary.TotalPurchasedCost = exchangeRate.Quantity * exchangeRate.Rate;
                    exchangeRateSummary.TotalPurchasedQuantity = exchangeRate.Quantity;
                    break;
                case 4:
                    exchangeRateSummary.TotalSoldCost = exchangeRate.Quantity * exchangeRate.Rate;
                    exchangeRateSummary.TotalSoldQuantity = exchangeRate.Quantity;
                    break;
            }

            if (summary is not null)
            {
                switch (exchangeRate.TransactionStatusId)
                {
                    case 3:
                        summary.TotalPurchasedCost += exchangeRateSummary.TotalPurchasedCost;
                        summary.TotalPurchasedQuantity += exchangeRateSummary.TotalPurchasedQuantity;
                        summary.AvgPurchasedRate = summary.TotalPurchasedCost / summary.TotalPurchasedQuantity;
                        break;
                    case 4:
                        summary.TotalSoldCost += exchangeRateSummary.TotalSoldCost;
                        summary.TotalSoldQuantity += exchangeRateSummary.TotalSoldQuantity;
                        summary.AvgSoldRate = summary.TotalSoldCost / summary.TotalSoldQuantity;
                        break;
                }
            }
            else
            {
                switch (exchangeRate.TransactionStatusId)
                {
                    case 3:
                        exchangeRateSummary.AvgPurchasedRate = exchangeRateSummary.TotalPurchasedCost / exchangeRateSummary.TotalPurchasedQuantity;
                        break;
                    case 4:
                        exchangeRateSummary.AvgSoldRate = exchangeRateSummary.TotalSoldCost / exchangeRateSummary.TotalSoldQuantity;
                        break;
                }
                await unitOfWork.ExchangeRateSummary.CreateEntityAsync(exchangeRateSummary).ConfigureAwait(false);
            }
        }
    }
}
