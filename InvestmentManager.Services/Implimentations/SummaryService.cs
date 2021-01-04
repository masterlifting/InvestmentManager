﻿using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Implimentations
{
    public class SummaryService : ISummaryService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public SummaryService(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

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
            var companySummaries = await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.ActualLot > 0).ToArrayAsync().ConfigureAwait(false);
            if (companySummaries is null || !companySummaries.Any())
                return 0;

            var actualPrices = unitOfWork.Price.GetLastPrices(30);
            return companySummaries.Join(actualPrices, x => x.CompanyId, y => y.Key, (x, y) => x.ActualLot * y.Value).Sum();
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
                summary.DateUpdate = DateTime.Now;
            }
            else if (transaction.TransactionStatusId == (long)TransactionStatusTypes.Add)
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = transaction.AccountId,
                    CurrencyId = transaction.CurrencyId,
                    InvestedSum = transaction.Amount
                }).ConfigureAwait(false);

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetAccountSummaryAsync(IEnumerable<AccountTransaction> transactions)
        {
            if (transactions is null || !transactions.Any())
                return true;

            try
            {
                foreach (var dataByAccount in transactions.GroupBy(x => x.AccountId))
                    foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                    {
                        decimal addedSum = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Add).Sum(x => x.Amount);
                        decimal withdrawedSum = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Withdraw).Sum(x => x.Amount);

                        await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                        {
                            AccountId = dataByAccount.Key,
                            CurrencyId = dataByCurrency.Key,
                            InvestedSum = addedSum - withdrawedSum
                        }).ConfigureAwait(false);
                    }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
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
                summaryIn.DateUpdate = DateTime.Now;
            }
            else if (exchangeRate.TransactionStatusId == (long)TransactionStatusTypes.Buy)
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = exchangeRate.AccountId,
                    CurrencyId = exchangeRate.CurrencyId,
                    InvestedSum = exchangeRate.Quantity
                }).ConfigureAwait(false);

            var summaryOut = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == (long)CurrencyTypes.rub).ConfigureAwait(false);

            if (summaryOut is not null)
            {
                summaryOut.InvestedSum = exchangeRate.TransactionStatusId switch
                {
                    (long)TransactionStatusTypes.Buy => summaryOut.InvestedSum - exchangeRate.Quantity * exchangeRate.Rate,
                    (long)TransactionStatusTypes.Sell => summaryOut.InvestedSum + exchangeRate.Quantity * exchangeRate.Rate,
                    _ => throw new NotImplementedException()
                };
                summaryOut.DateUpdate = DateTime.Now;
            }
            else if (exchangeRate.TransactionStatusId == (long)TransactionStatusTypes.Sell)
                await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                {
                    AccountId = exchangeRate.AccountId,
                    CurrencyId = (long)CurrencyTypes.rub,
                    InvestedSum = exchangeRate.Quantity * exchangeRate.Rate
                }).ConfigureAwait(false);

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetAccountSummaryAsync(IEnumerable<ExchangeRate> exchangeRates)
        {
            if (exchangeRates is null || !exchangeRates.Any())
                return true;

            try
            {
                foreach (var dataByAccount in exchangeRates.GroupBy(x => x.AccountId))
                    foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                    {
                        var summaryIn = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == dataByAccount.Key && x.CurrencyId == dataByCurrency.Key).ConfigureAwait(false);

                        decimal summaryInInvestedSumByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity);
                        decimal summaryInInvestedSumBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity);

                        if (summaryIn is not null)
                        {
                            summaryIn.InvestedSum += summaryInInvestedSumByBuy - summaryInInvestedSumBySell;
                            summaryIn.DateUpdate = DateTime.Now;
                        }
                        else
                        {
                            await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                            {
                                AccountId = dataByAccount.Key,
                                CurrencyId = dataByCurrency.Key,
                                InvestedSum = summaryInInvestedSumByBuy - summaryInInvestedSumBySell
                            }).ConfigureAwait(false);
                        }

                        var summaryOut = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == dataByAccount.Key && x.CurrencyId == (long)CurrencyTypes.rub).ConfigureAwait(false);

                        decimal summaryOutInvestedSumByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity * x.Rate);
                        decimal summaryOutInvestedSumBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity * x.Rate);

                        if (summaryOut is not null)
                        {
                            summaryOut.InvestedSum += summaryOutInvestedSumBySell - summaryOutInvestedSumByBuy;
                            summaryOut.DateUpdate = DateTime.Now;
                        }
                        else
                        {
                            await unitOfWork.AccountSummary.CreateEntityAsync(new AccountSummary
                            {
                                AccountId = dataByAccount.Key,
                                CurrencyId = (long)CurrencyTypes.rub,
                                InvestedSum = summaryOutInvestedSumBySell - summaryOutInvestedSumByBuy
                            }).ConfigureAwait(false);
                        }
                    }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }

        }
        public async Task<bool> SetCompanySummaryAsync(StockTransaction transaction)
        {
            if (transaction is null)
                return false;

            var companyId = (await unitOfWork.Ticker.FindByIdAsync(transaction.TickerId).ConfigureAwait(false))?.CompanyId;
            if (!companyId.HasValue)
                return false;

            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == transaction.AccountId &&  x.CompanyId == companyId.Value).ConfigureAwait(false);

            if (summary is not null)
            {
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
                summary.DateUpdate = DateTime.Now;
            }
            else if (transaction.TransactionStatusId == (long)TransactionStatusTypes.Buy)
                await unitOfWork.CompanySummary.CreateEntityAsync(new CompanySummary
                {
                    CompanyId = companyId.Value,
                    AccountId = transaction.AccountId,
                    CurrencyId = transaction.CurrencyId,
                    ActualLot = transaction.Quantity,
                    CurrentProfit = transaction.Quantity * transaction.Cost * -1
                }).ConfigureAwait(false);

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetCompanySummaryAsync(IEnumerable<StockTransaction> transactions)
        {
            if (transactions is null || !transactions.Any())
                return true;

            try
            {
                foreach (var dataByCompany in transactions.Join(unitOfWork.Ticker.GetAll(), x => x.TickerId, y => y.Id, (x, y) => new { Transaction = x, y.CompanyId }).GroupBy(x => x.CompanyId))
                    foreach (var dataByAccount in dataByCompany.Select(x => x.Transaction).GroupBy(x => x.AccountId))
                        foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                        {
                            decimal actualLotByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity);
                            decimal actualLotBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity);
                            decimal currentProfitByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity * x.Cost);
                            decimal currentProfitBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity * x.Cost);

                            await unitOfWork.CompanySummary.CreateEntityAsync(new CompanySummary
                            {
                                AccountId = dataByAccount.Key,
                                CompanyId = dataByCompany.Key,
                                CurrencyId = dataByCurrency.Key,
                                ActualLot = actualLotByBuy - actualLotBySell,
                                CurrentProfit = currentProfitBySell - currentProfitByBuy
                            }).ConfigureAwait(false);
                        }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
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
                summary.DateUpdate = DateTime.Now;
            }
            else
                await unitOfWork.DividendSummary.CreateEntityAsync(new DividendSummary
                {
                    AccountId = dividend.AccountId,
                    CurrencyId = dividend.CurrencyId,
                    CompanyId = companyId.Value,
                    TotalSum = dividend.Amount,
                    TotalTax = dividend.Tax
                }).ConfigureAwait(false);

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetDividendSummaryAsync(IEnumerable<Dividend> dividends)
        {
            if (dividends is null || !dividends.Any())
                return true;

            try
            {
                foreach (var dataByCompany in dividends.Join(unitOfWork.Isin.GetAll(), x => x.IsinId, y => y.Id, (x, y) => new { Dividend = x, y.CompanyId }).GroupBy(x => x.CompanyId))
                    foreach (var dataByAccount in dataByCompany.Select(x => x.Dividend).GroupBy(x => x.AccountId))
                        foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                        {
                            await unitOfWork.DividendSummary.CreateEntityAsync(new DividendSummary
                            {
                                AccountId = dataByAccount.Key,
                                CompanyId = dataByCompany.Key,
                                CurrencyId = dataByCurrency.Key,
                                TotalSum = dataByCurrency.Sum(x => x.Amount),
                                TotalTax = dataByCurrency.Sum(x => x.Tax)
                            }).ConfigureAwait(false);
                        }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> SetComissionSummaryAsync(Comission comission)
        {
            if (comission is null)
                return false;

            var summary = await unitOfWork.ComissionSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == comission.AccountId && x.CurrencyId == comission.CurrencyId).ConfigureAwait(false);

            if (summary is not null)
            {
                summary.TotalSum += comission.Amount;
                summary.DateUpdate = DateTime.Now;
            }
            else
                await unitOfWork.ComissionSummary.CreateEntityAsync(new ComissionSummary
                {
                    AccountId = comission.AccountId,
                    CurrencyId = comission.CurrencyId,
                    TotalSum = comission.Amount
                }).ConfigureAwait(false);

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetComissionSummaryAsync(IEnumerable<Comission> comissions)
        {
            if (comissions is null || !comissions.Any())
                return true;

            try
            {
                foreach (var dataByAccount in comissions.GroupBy(x => x.AccountId))
                    foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                    {
                        await unitOfWork.ComissionSummary.CreateEntityAsync(new ComissionSummary
                        {
                            AccountId = dataByAccount.Key,
                            CurrencyId = dataByCurrency.Key,
                            TotalSum = dataByCurrency.Sum(x => x.Amount)
                        }).ConfigureAwait(false);
                    }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
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
                summary.DateUpdate = DateTime.Now;

                return true;
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

            return await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task<bool> SetExchangeRateSummaryAsync(IEnumerable<ExchangeRate> exchangeRates)
        {
            if (exchangeRates is null || !exchangeRates.Any())
                return true;

            try
            {
                foreach (var dataByAccount in exchangeRates.GroupBy(x => x.AccountId))
                    foreach (var dataByCurrency in dataByAccount.GroupBy(x => x.CurrencyId))
                    {
                        var totalPurchasedCost = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity * x.Rate);
                        var totalPurchasedQuantity = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity);
                        var totalSoldCost = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity * x.Rate);
                        var totalSoldQuantity = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity);

                        await unitOfWork.ExchangeRateSummary.CreateEntityAsync(new ExchangeRateSummary
                        {
                            AccountId = dataByAccount.Key,
                            CurrencyId = dataByCurrency.Key,
                            TotalPurchasedCost = totalPurchasedCost,
                            TotalPurchasedQuantity = totalPurchasedQuantity,
                            TotalSoldCost = totalSoldCost,
                            TotalSoldQuantity = totalSoldQuantity,
                            AvgPurchasedRate = totalPurchasedCost / totalPurchasedQuantity,
                            AvgSoldRate = totalSoldCost / totalSoldQuantity
                        }).ConfigureAwait(false);
                    }

                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
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
                decimal accountComissionSum = 0;
                if (currencyId == (long)CurrencyTypes.rub)
                {
                    var comissions = await unitOfWork.ComissionSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId).ConfigureAwait(false);
                    if (comissions is not null)
                        accountComissionSum = comissions.TotalSum;
                }

                decimal result = accountSummary.InvestedSum + companiesOriginalInvestedSum + companiesFixedProfitSum + companiesDividendSum - accountComissionSum;

                accountSummary.FreeSum = result;
                accountSummary.DateUpdate = DateTime.Now;
                return await unitOfWork.CompleteAsync().ConfigureAwait(false);
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
            ExchangeRate[] exchangeRates;

            try
            {
                accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                exchangeRates = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
            }
            catch
            {
                return false;
            }
            #endregion

            #region delete old
            var accountSummary = unitOfWork.AccountSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var companySummary = unitOfWork.CompanySummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var dividendSummary = unitOfWork.DividendSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var comissionSummary = unitOfWork.ComissionSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var exchangeRateSummary = unitOfWork.ExchangeRateSummary.GetAll().Where(x => accountIds.Contains(x.AccountId));

            unitOfWork.AccountSummary.DeleteEntities(accountSummary);
            unitOfWork.CompanySummary.DeleteEntities(companySummary);
            unitOfWork.DividendSummary.DeleteEntities(dividendSummary);
            unitOfWork.ComissionSummary.DeleteEntities(comissionSummary);
            unitOfWork.ExchangeRateSummary.DeleteEntities(exchangeRateSummary);

            if (!await unitOfWork.CompleteAsync().ConfigureAwait(false))
                return false;
            #endregion

            #region calculating
            try
            {
                if (!await SetAccountSummaryAsync(accountTransactions).ConfigureAwait(false)
                || !await SetCompanySummaryAsync(stockTransactions).ConfigureAwait(false)
                || !await SetDividendSummaryAsync(dividends).ConfigureAwait(false)
                || !await SetComissionSummaryAsync(comissions).ConfigureAwait(false)
                || !await SetExchangeRateSummaryAsync(exchangeRates).ConfigureAwait(false)
                || !await SetAccountSummaryAsync(exchangeRates).ConfigureAwait(false))
                    return false;

                foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false))
                    foreach (var accountId in accountIds)
                        if (!await SetAccountFreeSumAsync(accountId, currencyId).ConfigureAwait(false))
                            return false;

                return true;
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
                ExchangeRate[] exchangeRates;

                try
                {
                    accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                    exchangeRates = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync().ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
                #endregion

                #region calculating
                try
                {
                    if (!await SetAccountSummaryAsync(accountTransactions).ConfigureAwait(false)
                    || !await SetCompanySummaryAsync(stockTransactions).ConfigureAwait(false)
                    || !await SetDividendSummaryAsync(dividends).ConfigureAwait(false)
                    || !await SetComissionSummaryAsync(comissions).ConfigureAwait(false)
                    || !await SetExchangeRateSummaryAsync(exchangeRates).ConfigureAwait(false)
                    || !await SetAccountSummaryAsync(exchangeRates).ConfigureAwait(false))
                        return false;

                    foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false))
                        foreach (var accountId in accountIds)
                            if (!await SetAccountFreeSumAsync(accountId, currencyId).ConfigureAwait(false))
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
