using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

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


        public async Task<bool> SetAccountSummaryAsync(AccountTransaction transaction)
        {
            if (transaction is null)
                return false;

            var summary = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == transaction.AccountId && x.CurrencyId == transaction.CurrencyId).ConfigureAwait(false);

            if (summary is not null)
            {
                summary.InvestedSum = transaction.TransactionStatusId switch
                {
                    (long)TransactionStatusTypes.Add => summary.InvestedSum + transaction.Amount,
                    (long)TransactionStatusTypes.Withdraw => summary.InvestedSum - transaction.Amount,
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

            return true;
        }
        public async Task<bool> SetAccountSummaryAsync(ExchangeRate exchangeRate)
        {
            if (exchangeRate is null)
                return false;

            var summaryIn = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId).ConfigureAwait(false);

            if (summaryIn is not null)
            {
                summaryIn.InvestedSum = exchangeRate.TransactionStatusId switch
                {
                    (long)TransactionStatusTypes.Buy => summaryIn.InvestedSum + exchangeRate.Quantity,
                    (long)TransactionStatusTypes.Sell => summaryIn.InvestedSum - exchangeRate.Quantity,
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
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == (long)CurrencyTypes.RUB).ConfigureAwait(false);

            if (summaryOut is not null)
            {
                summaryOut.InvestedSum = exchangeRate.TransactionStatusId switch
                {
                    (long)TransactionStatusTypes.Buy => summaryOut.InvestedSum - exchangeRate.Quantity * exchangeRate.Rate,
                    (long)TransactionStatusTypes.Sell => summaryOut.InvestedSum + exchangeRate.Quantity * exchangeRate.Rate,
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = exchangeRate.AccountId,
                    CurrencyId = (long)CurrencyTypes.RUB,
                    InvestedSum = exchangeRate.Quantity * exchangeRate.Rate
                }).ConfigureAwait(false);
            }

            return true;
        }
        public async Task<bool> SetAccountFreeSumAsync(long accountId, long currencyId)
        {
            var accountSummary = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId).ConfigureAwait(false);

            if (accountSummary is not null)
            {
                var companySummary = await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId).ToListAsync().ConfigureAwait(false);
                var dividendSummary = unitOfWork.DividendSummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId);

                decimal companiesFixedProfitSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit > 0).Sum(x => x.CurrentProfit) : 0;
                decimal companiesOriginalInvestedSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit < 0).Sum(x => x.CurrentProfit) : 0;
                decimal companiesDividendSum = dividendSummary is not null ? await dividendSummary.SumAsync(x => x.TotalSum).ConfigureAwait(false) : 0;
                decimal companiesComissionSum = 0;
                if (currencyId == (long)CurrencyTypes.RUB)
                    companiesComissionSum = await unitOfWork.ComissionSummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId).SumAsync(x => x.TotalSum).ConfigureAwait(false);

                decimal result = accountSummary.InvestedSum - companiesOriginalInvestedSum + companiesFixedProfitSum + companiesDividendSum - companiesComissionSum;

                accountSummary.FreeSum = result;
            }

            return true;
        }

        public async Task<bool> SetCompanySummaryAsync(StockTransaction transaction)
        {
            if (transaction is null)
                return false;

            var companyId = (await unitOfWork.Ticker.FindByIdAsync(transaction.TickerId).ConfigureAwait(false))?.CompanyId;
            if (!companyId.HasValue)
                return false;

            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.CompanyId == companyId.Value).ConfigureAwait(false);

            if (summary is not null)
                switch (transaction.TransactionStatusId)
                {
                    case (long)TransactionStatusTypes.Buy:
                        summary.ActualLot += transaction.Quantity;
                        summary.CurrentProfit -= (transaction.Quantity * transaction.Cost);
                        break;
                    case (long)TransactionStatusTypes.Sell:
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

            return true;
        }
        public async Task<bool> SetDividendSummaryAsync(Dividend dividend)
        {
            if (dividend is null)
                return false;

            var companyId = (await unitOfWork.Isin.FindByIdAsync(dividend.IsinId).ConfigureAwait(false))?.CompanyId;
            if (!companyId.HasValue)
                return false;

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

            return true;
        }
        public async Task<bool> SetComissionSummaryAsync(Comission comission)
        {
            if (comission is null)
                return false;

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

            return true;
        }
        public async Task<bool> SetExchangeRateSummaryAsync(ExchangeRate exchangeRate)
        {
            if (exchangeRate is null)
                return false;

            var summary = await unitOfWork.ExchangeRateSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId).ConfigureAwait(false);

            var exchangeRateSummary = new ExchangeRateSummary
            {
                AccountId = exchangeRate.AccountId,
                CurrencyId = exchangeRate.CurrencyId
            };
            switch (exchangeRate.TransactionStatusId)
            {
                case (long)TransactionStatusTypes.Buy:
                    exchangeRateSummary.TotalPurchasedCost = exchangeRate.Quantity * exchangeRate.Rate;
                    exchangeRateSummary.TotalPurchasedQuantity = exchangeRate.Quantity;
                    break;
                case (long)TransactionStatusTypes.Sell:
                    exchangeRateSummary.TotalSoldCost = exchangeRate.Quantity * exchangeRate.Rate;
                    exchangeRateSummary.TotalSoldQuantity = exchangeRate.Quantity;
                    break;
            }

            if (summary is not null)
            {
                switch (exchangeRate.TransactionStatusId)
                {
                    case (long)TransactionStatusTypes.Buy:
                        summary.TotalPurchasedCost += exchangeRateSummary.TotalPurchasedCost;
                        summary.TotalPurchasedQuantity += exchangeRateSummary.TotalPurchasedQuantity;
                        summary.AvgPurchasedRate = summary.TotalPurchasedCost / summary.TotalPurchasedQuantity;
                        break;
                    case (long)TransactionStatusTypes.Sell:
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
                    case (long)TransactionStatusTypes.Buy:
                        exchangeRateSummary.AvgPurchasedRate = exchangeRateSummary.TotalPurchasedCost / exchangeRateSummary.TotalPurchasedQuantity;
                        break;
                    case (long)TransactionStatusTypes.Sell:
                        exchangeRateSummary.AvgSoldRate = exchangeRateSummary.TotalSoldCost / exchangeRateSummary.TotalSoldQuantity;
                        break;
                }
                await unitOfWork.ExchangeRateSummary.CreateEntityAsync(exchangeRateSummary).ConfigureAwait(false);
            }

            return true;
        }

        public async Task<bool> ResetSummaryDataAsync(string userId)
        {
            #region set data to calculate
            var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

            AccountTransaction[] accountTransactions;
            StockTransaction[] stockTransactions;
            Dividend[] dividends;
            Comission[] comissions;
            ExchangeRate[] exchangeRate;

            try
            {
                accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                exchangeRate = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
            #endregion

            #region set data to delete
            var accountSummary = unitOfWork.AccountSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var companySummary = unitOfWork.CompanySummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var dividendSummary = unitOfWork.DividendSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var comissionSummary = unitOfWork.ComissionSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var exchangeRateSummary = unitOfWork.ExchangeRateSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            #endregion

            #region calculating
            try
            {
                unitOfWork.AccountSummary.DeleteEntities(accountSummary);
                for (int i = 0; i < accountTransactions.Length; i++)
                    if (!await SetAccountSummaryAsync(accountTransactions[i]).ConfigureAwait(false))
                        return false;

                unitOfWork.CompanySummary.DeleteEntities(companySummary);
                for (int i = 0; i < stockTransactions.Length; i++)
                    if (!await SetCompanySummaryAsync(stockTransactions[i]).ConfigureAwait(false))
                        return false;

                unitOfWork.DividendSummary.DeleteEntities(dividendSummary);
                for (int i = 0; i < dividends.Length; i++)
                    if (!await SetDividendSummaryAsync(dividends[i]).ConfigureAwait(false))
                        return false;

                unitOfWork.ComissionSummary.DeleteEntities(comissionSummary);
                for (int i = 0; i < comissions.Length; i++)
                    if (!await SetComissionSummaryAsync(comissions[i]).ConfigureAwait(false))
                        return false;

                unitOfWork.ExchangeRateSummary.DeleteEntities(exchangeRateSummary);
                for (int i = 0; i < exchangeRate.Length; i++)
                {
                    if (!await SetAccountSummaryAsync(exchangeRate[i]).ConfigureAwait(false) || !await SetExchangeRateSummaryAsync(exchangeRate[i]).ConfigureAwait(false))
                        return false;
                }

                foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false))
                    foreach (var accountId in accountIds)
                        await SetAccountFreeSumAsync(accountId, currencyId).ConfigureAwait(false);

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
            #endregion
        }
        public async Task<bool> ResetSummaryDataAsync(DataBaseType dbType, string[] userIds)
        {
            #region delete all
            try
            {
                switch (dbType)
                {
                    case DataBaseType.Postgres:
                        {
                            unitOfWork.ComissionSummary.DeleteAndReseedPostgres();
                            unitOfWork.DividendSummary.DeleteAndReseedPostgres();
                            unitOfWork.ExchangeRateSummary.DeleteAndReseedPostgres();
                            unitOfWork.CompanySummary.DeleteAndReseedPostgres();
                            unitOfWork.AccountSummary.DeleteAndReseedPostgres();
                        }
                        break;
                    case DataBaseType.SQL:
                        {
                            unitOfWork.ComissionSummary.TruncateAndReseedSQL();
                            unitOfWork.DividendSummary.TruncateAndReseedSQL();
                            unitOfWork.ExchangeRateSummary.TruncateAndReseedSQL();
                            unitOfWork.CompanySummary.TruncateAndReseedSQL();
                            unitOfWork.AccountSummary.TruncateAndReseedSQL();
                        }
                        break;
                }
            }
            catch
            {
                return false;
            }
            #endregion

            foreach (var userId in userIds)
            {
                #region set data to calculate
                var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

                AccountTransaction[] accountTransactions;
                StockTransaction[] stockTransactions;
                Dividend[] dividends;
                Comission[] comissions;
                ExchangeRate[] exchangeRate;

                try
                {
                    accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    exchangeRate = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
                #endregion

                #region calculating
                try
                {
                    for (int i = 0; i < accountTransactions.Length; i++)
                        if (!await SetAccountSummaryAsync(accountTransactions[i]).ConfigureAwait(false))
                            return false;

                    for (int i = 0; i < stockTransactions.Length; i++)
                        if (!await SetCompanySummaryAsync(stockTransactions[i]).ConfigureAwait(false))
                            return false;

                    for (int i = 0; i < dividends.Length; i++)
                        if (!await SetDividendSummaryAsync(dividends[i]).ConfigureAwait(false))
                            return false;

                    for (int i = 0; i < comissions.Length; i++)
                        if (!await SetComissionSummaryAsync(comissions[i]).ConfigureAwait(false))
                            return false;

                    for (int i = 0; i < exchangeRate.Length; i++)
                    {
                        if (!await SetAccountSummaryAsync(exchangeRate[i]).ConfigureAwait(false) || !await SetExchangeRateSummaryAsync(exchangeRate[i]).ConfigureAwait(false))
                            return false;
                    }

                    foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false))
                        foreach (var accountId in accountIds)
                            await SetAccountFreeSumAsync(accountId, currencyId).ConfigureAwait(false);

                    if (!await unitOfWork.CompleteAsync().ConfigureAwait(false))
                        return false;
                }
                catch
                {
                    return false;
                }
                #endregion
            }

            return true;
        }
    }
}
