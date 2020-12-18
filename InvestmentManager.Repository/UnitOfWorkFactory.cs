﻿using InvestmentManager.Entities.Basic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Repository
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly InvestmentContext context;
        public UnitOfWorkFactory(
        InvestmentContext context
        , IAccountRepository account
        , IAccountSummaryRepository accountSummary
        , IAccountTransactionRepository accountTransaction
        , IComissionRepository comission
        , IComissionSummaryRepository comissionSummary
        , IComissionTypeRepository comissionType
        , IDividendRepository dividend
        , IDividendSummaryRepository dividendSummary
        , IIsinRepository isin
        , IExchangeRateRepository exchangeRate
        , IExchangeRateSummaryRepository exchangeRateSummary
        , IStockTransactionRepository stockTransaction
        , ITransactionStatusRepository transactionStatus
        , IPriceRepository price
        , ILotRepository lot
        , ITickerRepository ticker
        , ISectorRepository sector
        , IReportRepository report
        , ICompanyRepository company
        , ICompanySummaryRepository companySummary
        , IExchangeRepository exchange
        , IIndustryRepository industry
        , IReportSourceRepository reportSource
        , IRatingRepository rating
        , ICoefficientRepository coefficient
        , ISellRecommendationRepository sellRecommendation
        , IBuyRecommendationRepository buyRecommendation
        , ICurrencyRepository currency)
        {
            this.context = context;
            Account = account;
            AccountSummary = accountSummary;
            AccountTransaction = accountTransaction;
            Comission = comission;
            ComissionSummary = comissionSummary;
            ComissionType = comissionType;
            Dividend = dividend;
            DividendSummary = dividendSummary;
            Isin = isin;
            ExchangeRate = exchangeRate;
            ExchangeRateSummary = exchangeRateSummary;
            StockTransaction = stockTransaction;
            TransactionStatus = transactionStatus;
            Price = price;
            Lot = lot;
            Ticker = ticker;
            Sector = sector;
            Report = report;
            Company = company;
            CompanySummary = companySummary;
            Exchange = exchange;
            Industry = industry;
            ReportSource = reportSource;
            Rating = rating;
            Coefficient = coefficient;
            SellRecommendation = sellRecommendation;
            BuyRecommendation = buyRecommendation;
            Currency = currency;
        }

        public IAccountRepository Account { get; }
        public IAccountSummaryRepository AccountSummary { get; }
        public IAccountTransactionRepository AccountTransaction { get; }
        public IComissionRepository Comission { get; }
        public IComissionSummaryRepository ComissionSummary { get; }
        public IComissionTypeRepository ComissionType { get; }
        public IDividendRepository Dividend { get; }
        public IDividendSummaryRepository DividendSummary { get; }
        public IIsinRepository Isin { get; }
        public IExchangeRateRepository ExchangeRate { get; }
        public IExchangeRateSummaryRepository ExchangeRateSummary { get; }
        public IStockTransactionRepository StockTransaction { get; }
        public ITransactionStatusRepository TransactionStatus { get; }
        public IPriceRepository Price { get; }
        public ILotRepository Lot { get; }
        public ITickerRepository Ticker { get; }
        public ISectorRepository Sector { get; }
        public IReportRepository Report { get; }
        public ICompanyRepository Company { get; }
        public ICompanySummaryRepository CompanySummary { get; }
        public IExchangeRepository Exchange { get; }
        public IIndustryRepository Industry { get; }
        public IReportSourceRepository ReportSource { get; }
        public IRatingRepository Rating { get; }
        public ICoefficientRepository Coefficient { get; }
        public ISellRecommendationRepository SellRecommendation { get; }
        public IBuyRecommendationRepository BuyRecommendation { get; }
        public ICurrencyRepository Currency { get; }


        public async Task<bool> CompleteAsync()
        {
            try
            {
                return await context.SaveChangesAsync().ConfigureAwait(false) > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task CustomAllUpdateAsync<T>(IEnumerable<T> entities, WithDelete withDelete = WithDelete.False) where T : class, IBaseEntity
        {
            if (entities is null || !entities.Any())
                return;

            var entitiesToAdd = entities.Where(x => x.Id == default);
            await context.AddRangeAsync(entitiesToAdd).ConfigureAwait(false);

            var entitiesToUpdate = entities.Where(x => x.Id != default);
            context.UpdateRange(entitiesToUpdate);

            if (withDelete == WithDelete.True)
            {
                var dbIds = context.Set<T>().Select(x => x.Id);
                var currentIds = entities.Where(x => x.Id != default).Select(x => x.Id);

                var entitiesToDelete = dbIds.Except(currentIds);

                if (entitiesToDelete.Any())
                    context.RemoveRange(entitiesToDelete);
            }
        }
    }
    public enum WithDelete
    {
        True,
        False
    }
}
