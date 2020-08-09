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

        void CreateEntity(TEntity entity);
        void CreateEntities(IEnumerable<TEntity> entities);

        void UpdateEntity(TEntity entity);
        void UpdateEntities(IEnumerable<TEntity> entities);

        void DeleteEntity(TEntity entity);
        void DeleteEntities(IEnumerable<TEntity> entities);
        void TruncateAndReseed();
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
    public interface IIsinRepository : IRepository<Isin> { }
    public interface ITickerRepository : IRepository<Ticker>
    {
        IEnumerable<Ticker> GetPriceTikers();
        IQueryable<Ticker> GetTickerIncludedLot();
    }
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
        IDictionary<long, Report> GetLastReports();
        /// <summary>
        /// Четыре последних даты отчетов
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns>4 даты</returns>
        IQueryable<DateTime> GetLastFourDateReport(long companyId);
        IDictionary<long, DateTime> GetLastDateReports();
        DateTime GetLastDateReport(long companyId);
    }
    public interface IPriceRepository : IRepository<Price>
    {
        Task<IEnumerable<Price>> GetCustomPricesAsync(long companyId, int lastMonths, OrderType orderDate);
        IDictionary<long, IEnumerable<Price>> GetGroupedPrices(int lastMonths, OrderType orderDate);
        IDictionary<long, decimal> GetLastPrices(double lastDays);
        IQueryable<DateTime> GetLastDates(long tickerId, int count);
        int GetCompanyCountWithPrices();
    }
    // Calculate
    public interface IRatingRepository : IRepository<Rating> { }
    public interface ICoefficientRepository : IRepository<Coefficient>
    {
        Task<List<Coefficient>> GetSortedCoefficientsAsync(long companyId);
        Task<List<(decimal price, Report report)>> GetSortedCoefficientCalculatingDataAsync(long companyId);
        IDictionary<string, List<(DateTime dateReport, Coefficient coefficient)>> GetViewData();
    }
    public interface ISellRecommendationRepository : IRepository<SellRecommendation> { }
    public interface IBuyRecommendationRepository : IRepository<BuyRecommendation> { }
    // Common
    public interface ICurrencyRepository : IRepository<Currency> { }
    #endregion
}
