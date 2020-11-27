﻿using InvestmentManager.Entities.Basic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Repository
{
    public interface IUnitOfWorkFactory
    {
        // Broker
        IAccountRepository Account { get; }
        IAccountTransactionRepository AccountTransaction { get; }
        IComissionRepository Comission { get; }
        IComissionTypeRepository ComissionType { get; }
        IDividendRepository Dividend { get; }
        IIsinRepository Isin { get; }
        IExchangeRateRepository ExchangeRate { get; }
        IStockTransactionRepository StockTransaction { get; }
        ITransactionStatusRepository TransactionStatus { get; }

        // Market
        IPriceRepository Price { get; }
        ILotRepository Lot { get; }
        ITickerRepository Ticker { get; }
        ISectorRepository Sector { get; }
        IReportRepository Report { get; }
        ICompanyRepository Company { get; }
        IExchangeRepository Exchange { get; }
        IIndustryRepository Industry { get; }
        IReportSourceRepository ReportSource { get; }

        // Calculate
        IRatingRepository Rating { get; }
        ICoefficientRepository Coefficient { get; }
        ISellRecommendationRepository SellRecommendation { get; }
        IBuyRecommendationRepository BuyRecommendation { get; }
        // Common
        ICurrencyRepository Currency { get; }


        Task<bool> CompleteAsync();
        Task CustomAllUpdateAsync<T>(IEnumerable<T> entities, WithDelete withDelete = WithDelete.False) where T : class, IBaseEntity;
    }
}
