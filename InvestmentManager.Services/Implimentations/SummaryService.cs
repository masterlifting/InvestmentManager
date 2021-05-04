using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Models.Additional;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Implimentations
{
    public class SummaryService : ISummaryService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IWebService webService;

        public SummaryService(IUnitOfWorkFactory unitOfWork, IWebService webService)
        {
            this.unitOfWork = unitOfWork;
            this.webService = webService;
        }

        public async Task<decimal> GetAccountSumAsync(long accountId)
        {
            decimal result = 0;

            long[] currencyIds = await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync();

            try
            {
                foreach (var currencyId in currencyIds)
                {
                    decimal intermediateResult = await GetAccountSumAsync(accountId, currencyId);
                    decimal rateValue = 1;

                    if (currencyId != (long)CurrencyTypes.rub)
                    {
                        var response = await webService.GetCBRateAsync();
                        var rate = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CBRF>() : null;

                        rateValue = rate is not null
                            ? currencyId switch
                            {
                                (long)CurrencyTypes.usd => rate.Valute.USD.Value,
                                //(long)CurrencyTypes.RUB => rate.Valute.EUR.Value,
                                _ => 0
                            }
                            : 0;
                    }

                    result += intermediateResult * rateValue;
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }
        public async Task<decimal> GetAccountSumAsync(long accountId, long currencyId)
        {
            var accountSummary = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId);
            if (accountSummary is null)
                return 0;

            decimal companiesActualInvestedSum = await GetCompaniesActualInvestedSumAsync(accountId, currencyId);

            return companiesActualInvestedSum + accountSummary.FreeSum;
        }
        public async Task<decimal> GetAccountInvestedSumAsync(long accountId, long currencyId)
        {
            var result = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId);
            if (result is null)
                return 0;

            return result.InvestedSum;
        }

        public async Task<decimal> GetCompaniesFixedProfitSumAsync(long accountId, long currencyId) =>
            await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.CurrentProfit > 0).SumAsync(x => x.CurrentProfit);
        public async Task<decimal> GetCompaniesOriginalInvestedSumAsync(long accountId, long currencyId) =>
            (await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.CurrentProfit < 0).SumAsync(x => x.CurrentProfit)) * -1;
        public async Task<decimal> GetCompaniesActualInvestedSumAsync(long accountId, long currencyId)
        {
            var companySummaries = await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId && x.ActualLot > 0).ToArrayAsync();
            if (companySummaries is null || !companySummaries.Any())
                return 0;

            var actualPrices = await unitOfWork.Price.GetLastPricesAsync(30, companySummaries.Select(x => x.CompanyId));
            return companySummaries.Join(actualPrices, x => x.CompanyId, y => y.Key, (x, y) => x.ActualLot * y.Value).Sum();
        }
        public async Task<decimal> GetCompanyActualInvestedSumAsync(long companyId)
        {
            var companySummary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.CompanyId == companyId);
            if (companySummary.ActualLot == 0)
                return 0;

            var actualPrice = await unitOfWork.Price.GetCustomOrderedPricesAsync(companyId, 1);
            if (actualPrice is null)
                return 0;

            return companySummary.ActualLot * actualPrice.Last().Value;
        }


        public async Task<bool> SetAccountSummaryAsync(AccountTransaction transaction)
        {
            if (transaction is null)
                return false;

            var summary = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == transaction.AccountId && x.CurrencyId == transaction.CurrencyId);

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
                });

            return await unitOfWork.CompleteAsync();
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
                        });
                    }

                return await unitOfWork.CompleteAsync();
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
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId);

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
                });

            var summaryOut = await unitOfWork.AccountSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == (long)CurrencyTypes.rub);

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
                });

            return await unitOfWork.CompleteAsync();
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
                        var summaryIn = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == dataByAccount.Key && x.CurrencyId == dataByCurrency.Key);

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
                            });
                        }

                        var summaryOut = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == dataByAccount.Key && x.CurrencyId == (long)CurrencyTypes.rub);

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
                            });
                        }
                    }

                return await unitOfWork.CompleteAsync();
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

            var companyId = (await unitOfWork.Ticker.FindByIdAsync(transaction.TickerId))?.CompanyId;
            if (!companyId.HasValue)
                return false;

            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == transaction.AccountId && x.CompanyId == companyId.Value);

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
                });

            return await unitOfWork.CompleteAsync();
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
                            int actualLotByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity);
                            int actualLotBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity);
                            decimal currentProfitByBuy = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Buy).Sum(x => x.Quantity * x.Cost);
                            decimal currentProfitBySell = dataByCurrency.Where(x => x.TransactionStatusId == (long)TransactionStatusTypes.Sell).Sum(x => x.Quantity * x.Cost);

                            await unitOfWork.CompanySummary.CreateEntityAsync(new CompanySummary
                            {
                                AccountId = dataByAccount.Key,
                                CompanyId = dataByCompany.Key,
                                CurrencyId = dataByCurrency.Key,
                                ActualLot = actualLotByBuy - actualLotBySell,
                                CurrentProfit = currentProfitBySell - currentProfitByBuy
                            });
                        }

                return await unitOfWork.CompleteAsync();
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

            var companyId = (await unitOfWork.Isin.FindByIdAsync(dividend.IsinId))?.CompanyId;
            if (!companyId.HasValue)
                return false;

            var summary = await unitOfWork.DividendSummary.GetAll()
                .FirstOrDefaultAsync(x => x.AccountId == dividend.AccountId && x.CurrencyId == dividend.CurrencyId && x.CompanyId == companyId.Value);

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
                });

            return await unitOfWork.CompleteAsync();
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
                            });
                        }

                return await unitOfWork.CompleteAsync();
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
                .FirstOrDefaultAsync(x => x.AccountId == comission.AccountId && x.CurrencyId == comission.CurrencyId);

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
                });

            return await unitOfWork.CompleteAsync();
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
                        });
                    }

                return await unitOfWork.CompleteAsync();
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
                .FirstOrDefaultAsync(x => x.AccountId == exchangeRate.AccountId && x.CurrencyId == exchangeRate.CurrencyId);

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
                await unitOfWork.ExchangeRateSummary.CreateEntityAsync(exchangeRateSummary);
            }

            return await unitOfWork.CompleteAsync();
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
                        });
                    }

                return await unitOfWork.CompleteAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SetAccountFreeSumAsync(long accountId, long currencyId)
        {
            var accountSummary = await unitOfWork.AccountSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId);

            if (accountSummary is not null)
            {
                var companySummary = await unitOfWork.CompanySummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId).ToListAsync();
                var dividendSummary = unitOfWork.DividendSummary.GetAll().Where(x => x.AccountId == accountId && x.CurrencyId == currencyId);

                decimal companiesFixedProfitSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit > 0).Sum(x => x.CurrentProfit) : 0;
                decimal companiesOriginalInvestedSum = companySummary.Any() ? companySummary.Where(x => x.CurrentProfit < 0).Sum(x => x.CurrentProfit) : 0;
                decimal companiesDividendSum = dividendSummary is not null ? await dividendSummary.SumAsync(x => x.TotalSum) : 0;
                decimal accountComissionSum = 0;
                if (currencyId == (long)CurrencyTypes.rub)
                {
                    var comissions = await unitOfWork.ComissionSummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CurrencyId == currencyId);
                    if (comissions is not null)
                        accountComissionSum = comissions.TotalSum;
                }

                decimal result = accountSummary.InvestedSum + companiesOriginalInvestedSum + companiesFixedProfitSum + companiesDividendSum - accountComissionSum;

                accountSummary.FreeSum = result;
                accountSummary.DateUpdate = DateTime.Now;
                return await unitOfWork.CompleteAsync();
            }

            return true;
        }

        public async Task<bool> ResetSummaryDataAsync(string userId)
        {
            #region set data to calculate
            var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync();

            AccountTransaction[] accountTransactions;
            StockTransaction[] stockTransactions;
            Dividend[] dividends;
            Comission[] comissions;
            ExchangeRate[] exchangeRates;

            try
            {
                accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                exchangeRates = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
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

            if (!await unitOfWork.CompleteAsync())
                return false;
            #endregion

            #region calculating
            try
            {
                if (!await SetAccountSummaryAsync(accountTransactions)
                || !await SetCompanySummaryAsync(stockTransactions)
                || !await SetDividendSummaryAsync(dividends)
                || !await SetComissionSummaryAsync(comissions)
                || !await SetExchangeRateSummaryAsync(exchangeRates)
                || !await SetAccountSummaryAsync(exchangeRates))
                    return false;

                foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync())
                    foreach (var accountId in accountIds)
                        if (!await SetAccountFreeSumAsync(accountId, currencyId))
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
                var accountIds = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id).ToArrayAsync();

                AccountTransaction[] accountTransactions;
                StockTransaction[] stockTransactions;
                Dividend[] dividends;
                Comission[] comissions;
                ExchangeRate[] exchangeRates;

                try
                {
                    accountTransactions = await unitOfWork.AccountTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                    stockTransactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                    dividends = await unitOfWork.Dividend.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                    comissions = await unitOfWork.Comission.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                    exchangeRates = await unitOfWork.ExchangeRate.GetAll().Where(x => accountIds.Contains(x.AccountId)).OrderBy(x => x.DateOperation).ToArrayAsync();
                }
                catch
                {
                    return false;
                }
                #endregion

                #region calculating
                try
                {
                    if (!await SetAccountSummaryAsync(accountTransactions)
                    || !await SetCompanySummaryAsync(stockTransactions)
                    || !await SetDividendSummaryAsync(dividends)
                    || !await SetComissionSummaryAsync(comissions)
                    || !await SetExchangeRateSummaryAsync(exchangeRates)
                    || !await SetAccountSummaryAsync(exchangeRates))
                        return false;

                    foreach (var currencyId in await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync())
                        foreach (var accountId in accountIds)
                            if (!await SetAccountFreeSumAsync(accountId, currencyId))
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
