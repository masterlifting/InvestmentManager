using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> FindByIdAsync(long id);
        IQueryable<TEntity> GetAll();

        Task CreateEntityAsync(TEntity entity);
        Task CreateEntitiesAsync(IEnumerable<TEntity> entities);

        void UpdateEntity(TEntity entity);
        void UpdateEntities(IEnumerable<TEntity> entities);

        void DeleteEntity(TEntity entity);
        void DeleteEntities(IEnumerable<TEntity> entities);
        void TruncateAndReseedSQL();
        void DeleteAndReseedPostgres();
        void PostgresAutoReseed();
        Task<bool> CompletePostgresAsync();
    }

    #region Repository Interfaces
    // Broker
    public interface IAccountRepository : IRepository<Account> { }
    public interface IAccountTransactionRepository : IRepository<AccountTransaction> { }
    public interface IComissionRepository : IRepository<Comission> { }
    public interface IComissionTypeRepository : IRepository<ComissionType> { }
    public interface IDividendRepository : IRepository<Dividend> { }
    public interface IExchangeRateRepository : IRepository<ExchangeRate> { }
    public interface IStockTransactionRepository : IRepository<StockTransaction> { }
    public interface ITransactionStatusRepository : IRepository<TransactionStatus> { }
    // Market
    public interface IExchangeRepository : IRepository<Exchange> { }
    public interface IWeekendRepository : IRepository<Weekend> { }
    public interface IIsinRepository : IRepository<Isin> { }
    public interface ITickerRepository : IRepository<Ticker> { }
    public interface ILotRepository : IRepository<Lot> { }
    public interface ICompanyRepository : IRepository<Company> { }
    public interface IIndustryRepository : IRepository<Industry> { }
    public interface ISectorRepository : IRepository<Sector> { }
    public interface IReportSourceRepository : IRepository<ReportSource> { }
    public interface IReportRepository : IRepository<Report>
    {
        /// <summary>
        /// Словарь последних вышедших отчетов по каждой компании
        /// </summary>
        /// <returns>Словарь(CompanyId,Report)</returns>
        Task<IDictionary<long, Report>> GetLastReportsAsync();
        /// <summary>
        /// Четыре последних даты отчетов
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns>4 даты</returns>
        Task<DateTime[]> GetLastFourDateReportAsync(long companyId);
    }
    public interface IPriceRepository : IRepository<Price>
    {
        Task<Price[]> GetCustomOrderedPricesAsync(long companyId, int lastMonths, DateTime? startDate = null);
        Task<Dictionary<long, Price[]>> GetGroupedOrderedPricesAsync(int lastMonths);
        Task<IDictionary<long, decimal>> GetLastPricesAsync(double lastDays);
        Task<IDictionary<long, decimal>> GetLastPricesAsync(double lastDays, IEnumerable<long> companyIds);
        Task<DateTime[]> GetLastDatesAsync(long tickerId, int count);
        Task<int> GetCompanyCountWithPricesAsync();
        Task<Ticker[]> GetTickersByPricesAsync();
    }
    // Calculate
    public interface IRatingRepository : IRepository<Rating> { }
    public interface ICoefficientRepository : IRepository<Coefficient> { }
    public interface ISellRecommendationRepository : IRepository<SellRecommendation> { }
    public interface IBuyRecommendationRepository : IRepository<BuyRecommendation> { }
    public interface IAccountSummaryRepository : IRepository<AccountSummary> { }
    public interface ICompanySummaryRepository : IRepository<CompanySummary> { }
    public interface IDividendSummaryRepository : IRepository<DividendSummary> { }
    public interface IComissionSummaryRepository : IRepository<ComissionSummary> { }
    public interface IExchangeRateSummaryRepository : IRepository<ExchangeRateSummary> { }
    // Common
    public interface ICurrencyRepository : IRepository<Currency> { }
    #endregion
}
