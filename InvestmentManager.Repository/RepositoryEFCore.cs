using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Repository
{
    public class RepositoryEFCore<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly InvestmentContext context;
        public RepositoryEFCore(InvestmentContext context) => this.context = context;

        public async Task<TEntity> FindByIdAsync(long id) => await context.Set<TEntity>().FindAsync(id).ConfigureAwait(false);
        public IQueryable<TEntity> GetAll() => context.Set<TEntity>();

        public async Task CreateEntityAsync(TEntity entity) => await context.Set<TEntity>().AddAsync(entity).ConfigureAwait(false);
        public async Task CreateEntitiesAsync(IEnumerable<TEntity> entities) => await context.Set<TEntity>().AddRangeAsync(entities).ConfigureAwait(false);

        public void UpdateEntity(TEntity entity) => context.Set<TEntity>().Update(entity);
        public void UpdateEntities(IEnumerable<TEntity> entities) => context.Set<TEntity>().UpdateRange(entities);

        public void DeleteEntity(TEntity entity) => context.Set<TEntity>().Remove(entity);
        public void DeleteEntities(IEnumerable<TEntity> entities) => context.Set<TEntity>().RemoveRange(entities);
        public void TruncateAndReseedSQL()
        {
            var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            context.Database.ExecuteSqlRaw($"TRUNCATE TABLE {tableName}");
            context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('{tableName}', RESEED, 1)");
        }
        public void DeleteAndReseedPostgres()
        {
            var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            context.Database.ExecuteSqlRaw($"DELETE FROM \"{tableName}\"");
            context.Database.ExecuteSqlRaw($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH 1");
        }

        public void PostgresAutoReseed()
        {
            var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            var idlimit = context.Set<TEntity>().AsEnumerable().Max(x => x.GetType().GetProperty("Id").GetValue(x));
            long nextId = (long)idlimit + 1;
            context.Database.ExecuteSqlRaw($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH {nextId}");
        }
    }
    public enum OrderType
    {
        OrderBy,
        OrderByDesc
    }

    #region Repository Classes
    // Broker
    public class AccountRepository : RepositoryEFCore<Account>, IAccountRepository { public AccountRepository(InvestmentContext context) : base(context) { } }
    public class AccountTransactionRepository : RepositoryEFCore<AccountTransaction>, IAccountTransactionRepository { public AccountTransactionRepository(InvestmentContext context) : base(context) { } }
    public class ComissionRepository : RepositoryEFCore<Comission>, IComissionRepository { public ComissionRepository(InvestmentContext context) : base(context) { } }
    public class ComissionTypeRepository : RepositoryEFCore<ComissionType>, IComissionTypeRepository { public ComissionTypeRepository(InvestmentContext context) : base(context) { } }
    public class DividendRepository : RepositoryEFCore<Dividend>, IDividendRepository { public DividendRepository(InvestmentContext context) : base(context) { } }
    public class ExchangeRateRepository : RepositoryEFCore<ExchangeRate>, IExchangeRateRepository { public ExchangeRateRepository(InvestmentContext context) : base(context) { } }
    public class StockTransactionRepository : RepositoryEFCore<StockTransaction>, IStockTransactionRepository { public StockTransactionRepository(InvestmentContext context) : base(context) { } }
    public class TransactionStatusRepository : RepositoryEFCore<TransactionStatus>, ITransactionStatusRepository { public TransactionStatusRepository(InvestmentContext context) : base(context) { } }
    // Market
    public class ExchangeRepository : RepositoryEFCore<Exchange>, IExchangeRepository { public ExchangeRepository(InvestmentContext context) : base(context) { } }
    public class IsinRepository : RepositoryEFCore<Isin>, IIsinRepository { public IsinRepository(InvestmentContext context) : base(context) { } }
    public class TickerRepository : RepositoryEFCore<Ticker>, ITickerRepository
    {
        private readonly InvestmentContext context;
        public TickerRepository(InvestmentContext context) : base(context) => this.context = context;

        public IQueryable<Ticker> GetTickerIncludedLot() => context.Tickers.AsNoTracking().Include(x => x.Lot);
        public IEnumerable<Ticker> GetPriceTikers() => context.Tickers.AsNoTracking().AsEnumerable().GroupBy(x => x.CompanyId).Select(x => x.First());
    }
    public class LotRepository : RepositoryEFCore<Lot>, ILotRepository { public LotRepository(InvestmentContext context) : base(context) { } }
    public class CompanyRepository : RepositoryEFCore<Company>, ICompanyRepository { public CompanyRepository(InvestmentContext context) : base(context) { } }
    public class IndustryRepository : RepositoryEFCore<Industry>, IIndustryRepository { public IndustryRepository(InvestmentContext context) : base(context) { } }
    public class SectorRepository : RepositoryEFCore<Sector>, ISectorRepository { public SectorRepository(InvestmentContext context) : base(context) { } }
    public class ReportSourceRepository : RepositoryEFCore<ReportSource>, IReportSourceRepository { public ReportSourceRepository(InvestmentContext context) : base(context) { } }
    public class ReportRepository : RepositoryEFCore<Report>, IReportRepository
    {
        private readonly InvestmentContext context;
        public ReportRepository(InvestmentContext context) : base(context) => this.context = context;

        public IDictionary<long, Report> GetLastReports()
        {
            var result = new Dictionary<long, Report>();

            var source = context.Reports.AsNoTracking().Where(x => x.DateReport >= DateTime.Now.AddMonths(-4)).OrderBy(x => x.DateReport);
            var agregatedData = source.AsEnumerable().GroupBy(x => x.CompanyId).Select(x => new { CompanyId = x.Key, LastReport = x.Last() });

            foreach (var i in agregatedData)
                result.Add(i.CompanyId, i.LastReport);

            return result;
        }
        public IQueryable<DateTime> GetLastFourDateReport(long companyId) => context.Reports.AsNoTracking()
            .Where(x => x.CompanyId == companyId).OrderByDescending(x => x.DateReport).Select(x => x.DateReport).Take(4);
        public IDictionary<long, DateTime> GetLastDateReports()
        {
            var result = new Dictionary<long, DateTime>();

            var source = context.Reports.AsNoTracking().OrderBy(x => x.DateReport);
            var agregatedData = source.AsEnumerable().GroupBy(x => x.CompanyId).Select(x => new { CompanyId = x.Key, LastDate = x.Last().DateReport });

            foreach (var i in agregatedData)
                result.Add(i.CompanyId, i.LastDate);

            return result;
        }
        public DateTime GetLastDateReport(long companyId) => context.Reports.AsNoTracking().Where(x => x.CompanyId == companyId).OrderBy(x => x.DateReport).Last().DateReport;
    }
    public class PriceRepository : RepositoryEFCore<Price>, IPriceRepository
    {
        private readonly InvestmentContext context;
        public PriceRepository(InvestmentContext context) : base(context) => this.context = context;

        public IDictionary<long, IEnumerable<Price>> GetGroupedPrices(int lastMonths, OrderType orderDate)
        {
            var result = new Dictionary<long, IEnumerable<Price>>();

            var tickers = context.Companies.AsNoTracking().Include(x => x.Tickers).Select(x => x.Tickers.First());
            var prices = orderDate == OrderType.OrderBy
                ? context.Prices.AsNoTracking().Where(x => x.BidDate >= DateTime.Now.AddMonths(-lastMonths)).OrderBy(x => x.BidDate)
                : context.Prices.AsNoTracking().Where(x => x.BidDate >= DateTime.Now.AddMonths(-lastMonths)).OrderByDescending(x => x.BidDate);

            var agregatedData = prices.AsEnumerable().GroupBy(x => x.TickerId).Join(tickers, x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, Prices = x.Select(y => y) });

            foreach (var i in agregatedData)
                result.Add(i.CompanyId, i.Prices);

            return result;
        }
        public IDictionary<long, decimal> GetLastPrices(double lastDays)
        {
            var result = new Dictionary<long, decimal>();

            var tickers = context.Companies.AsNoTracking().Include(x => x.Tickers).Select(x => x.Tickers.First());
            var prices = context.Prices.AsNoTracking().Where(x => x.BidDate >= DateTime.Now.AddDays(-lastDays)).OrderBy(x => x.BidDate);
            var agregatedData = prices.AsEnumerable().GroupBy(x => x.TickerId).Join(tickers, x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, LastPrice = x.Last().Value });

            foreach (var i in agregatedData)
                result.Add(i.CompanyId, i.LastPrice);

            return result;
        }
        public async Task<IEnumerable<Price>> GetCustomPricesAsync(long companyId, int lastMonths, OrderType orderDate) => orderDate == OrderType.OrderByDesc
            ? (await context.Companies.AsNoTracking().Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
            .Tickers.First().Prices.Where(x => x.BidDate >= DateTime.Now.AddMonths(-lastMonths)).OrderByDescending(x => x.BidDate)
            : (await context.Companies.AsNoTracking().Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
            .Tickers.First().Prices.Where(x => x.BidDate >= DateTime.Now.AddMonths(-lastMonths)).OrderBy(x => x.BidDate);

        public IQueryable<DateTime> GetLastDates(long tickerId, int count) =>
            context.Prices.Where(x => x.TickerId == tickerId).OrderByDescending(x => x.BidDate).Select(x => x.BidDate.Date).Take(count);
        public int GetCompanyCountWithPrices() => context.Companies.AsNoTracking().Include(x => x.Tickers).ThenInclude(x => x.Prices).Select(x => x.Tickers.First()).Where(x => x.Prices.Any()).Count();
    }
    // Calculate
    public class RatingRepository : RepositoryEFCore<Rating>, IRatingRepository { public RatingRepository(InvestmentContext context) : base(context) { } }
    public class CoefficientRepository : RepositoryEFCore<Coefficient>, ICoefficientRepository
    {
        private readonly InvestmentContext context;
        public CoefficientRepository(InvestmentContext context) : base(context) => this.context = context;

        public async Task<List<(decimal, Report)>> GetSortedCoefficientCalculatingDataAsync(long companyId)
        {
            var result = new List<(decimal, Report)>();

            foreach (var i in (await context.Companies.AsNoTracking().Include(x => x.Reports).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false)).Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport))
            {
                var prices = (await context.Companies.AsNoTracking().Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == i.CompanyId).ConfigureAwait(false))
                    .Tickers.FirstOrDefault().Prices.Where(x => x.BidDate <= i.DateReport);

                result.Add((prices.Any() ? prices.OrderByDescending(x => x.BidDate).FirstOrDefault().Value : 0, i));
            }

            return result;
        }
        public IDictionary<string, List<(DateTime, Coefficient)>> GetViewData()
        {
            var result = new Dictionary<string, List<(DateTime, Coefficient)>>();

            foreach (var i in context.Companies.AsNoTracking().Include(x => x.Reports).ThenInclude(x => x.Coefficient))
            {
                var temp = new List<(DateTime, Coefficient)>();

                foreach (var j in i.Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport))
                {
                    temp.Add((j.DateReport, j.Coefficient));
                }

                result.Add(i.Name, temp);
            }

            return result;
        }
        public async Task<List<Coefficient>> GetSortedCoefficientsAsync(long companyId) =>
            (await context.Companies.AsNoTracking().Include(x => x.Reports).ThenInclude(x => x.Coefficient).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
            .Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport).Select(x => x.Coefficient).ToList();
    }
    public class SellRecommendationRepository : RepositoryEFCore<SellRecommendation>, ISellRecommendationRepository { public SellRecommendationRepository(InvestmentContext context) : base(context) { } }
    public class BuyRecommendationRepository : RepositoryEFCore<BuyRecommendation>, IBuyRecommendationRepository { public BuyRecommendationRepository(InvestmentContext context) : base(context) { } }
    // Common
    public class CurrencyRepository : RepositoryEFCore<Currency>, ICurrencyRepository { public CurrencyRepository(InvestmentContext context) : base(context) { } }
    #endregion
}
