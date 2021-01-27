using InvestmentManager.Entities.Basic;
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
        IWeekendRepository Weekend { get; }
        IIndustryRepository Industry { get; }
        IReportSourceRepository ReportSource { get; }

        // Calculate
        IRatingRepository Rating { get; }
        ICoefficientRepository Coefficient { get; }
        ISellRecommendationRepository SellRecommendation { get; }
        IBuyRecommendationRepository BuyRecommendation { get; }
        ICompanySummaryRepository CompanySummary { get; }
        IAccountSummaryRepository AccountSummary { get; }
        IDividendSummaryRepository DividendSummary { get; }
        IComissionSummaryRepository ComissionSummary { get; }
        IExchangeRateSummaryRepository ExchangeRateSummary { get; }
        // Common
        ICurrencyRepository Currency { get; }


        Task<bool> CompleteAsync();
        /// <summary>
        /// Add new, Update current, Delete old
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns>Task</returns>
        Task<bool> CustomUpdateRangeAsync<T>(IEnumerable<T> entities) where T : class, IBaseEntity;
    }
}
