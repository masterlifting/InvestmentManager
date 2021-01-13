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
        public async Task<bool> CompletePostgresAsync()
        {
            try
            {
                return await context.SaveChangesAsync().ConfigureAwait(false) > 0;
            }
            catch
            {
                try
                {
                    PostgresAutoReseed();
                    return await context.SaveChangesAsync().ConfigureAwait(false) > 0;
                }
                catch
                {
                    return false;
                }
            }
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
    }
    public class PriceRepository : RepositoryEFCore<Price>, IPriceRepository
    {
        private readonly InvestmentContext context;
        public PriceRepository(InvestmentContext context) : base(context) => this.context = context;

        public async Task<Dictionary<long, Price[]>> GetGroupedPricesAsync(int lastMonths, OrderType orderDate)
        {
            var result = new Dictionary<long, Price[]>();
            DateTime baseStartDate = DateTime.Now.AddMonths(-lastMonths);

            var tickers = await GetTickersByPricesAsync().ConfigureAwait(false);
            var priceQuery = context.Prices.Where(x => x.BidDate >= baseStartDate);

            var prices = orderDate == OrderType.OrderBy
                ? (await priceQuery.OrderBy(x => x.BidDate).ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.TickerId)
                : (await priceQuery.OrderByDescending(x => x.BidDate).ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.TickerId);

            var agregatedData = tickers
                .Join(context.Companies, x => x.CompanyId, y => y.Id, (x, y) => new { TickerId = x.Id, x.CompanyId, y.DateSplit })
                .Join(prices, x => x.TickerId, y => y.Key, (x, y) => (x.CompanyId, x.DateSplit, Prices: y.Select(z => z).ToArray()));


            foreach (var i in agregatedData)
            {
                if (i.DateSplit.HasValue)
                {
                    DateTime startDateSplit = i.DateSplit.Value > baseStartDate ? i.DateSplit.Value : baseStartDate;
                    result.Add(i.CompanyId, i.Prices.Where(x => x.BidDate >= startDateSplit).ToArray());
                }
                else
                    result.Add(i.CompanyId, i.Prices);
            }

            return result;
        }
        public async Task<IDictionary<long, decimal>> GetLastPricesAsync(double lastDays)
        {
            var result = new Dictionary<long, decimal>();

            var tickers = await GetTickersByPricesAsync().ConfigureAwait(false);
            var prices = await context.Prices.Where(x => x.BidDate >= DateTime.Now.AddDays(-lastDays)).OrderByDescending(x => x.BidDate).ToArrayAsync().ConfigureAwait(false);

            var agregatedData = prices.GroupBy(x => x.TickerId).Join(tickers, x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, LastPrice = x.First().Value });
            foreach (var i in agregatedData)
                result.Add(i.CompanyId, i.LastPrice);

            return result;
        }
        public async Task<Price[]> GetCustomPricesAsync(long companyId, int lastMonths, OrderType orderDate, DateTime? startDate = null)
        {
            var result = Array.Empty<Price>();

            var ticker = await context.Tickers.FirstOrDefaultAsync(x => x.CompanyId == companyId).ConfigureAwait(false);
            if (ticker is null)
                return result;

            DateTime baseStartDate = DateTime.Now.AddMonths(-lastMonths);
            DateTime resultStartDate = startDate.HasValue
                ? startDate.Value > baseStartDate
                ? startDate.Value.AddDays(1)
                : baseStartDate
                : baseStartDate;

            var prices = context.Prices.Where(x => x.TickerId == ticker.Id && x.BidDate >= resultStartDate);
            if (prices is null || !prices.Any())
                return result;

            return orderDate == OrderType.OrderByDesc
                ? await prices.OrderByDescending(x => x.BidDate).ToArrayAsync().ConfigureAwait(false)
                : await prices.OrderBy(x => x.BidDate).ToArrayAsync().ConfigureAwait(false);
        }
        public async Task<DateTime[]> GetLastDatesAsync(long tickerId, int count) =>
            await context.Prices.Where(x => x.TickerId == tickerId).OrderByDescending(x => x.BidDate).Take(count).Select(x => x.BidDate).ToArrayAsync().ConfigureAwait(false);
        public async Task<int> GetCompanyCountWithPricesAsync() =>
            await context.Prices.Select(x => x.TickerId).Distinct().CountAsync().ConfigureAwait(false);

        private async Task<Ticker[]> GetTickersByPricesAsync() =>
            (await context.Tickers.ToArrayAsync().ConfigureAwait(false)).GroupBy(x => x.CompanyId).Select(x => x.First()).ToArray();

    }
    // Calculate
    public class RatingRepository : RepositoryEFCore<Rating>, IRatingRepository { public RatingRepository(InvestmentContext context) : base(context) { } }
    public class CoefficientRepository : RepositoryEFCore<Coefficient>, ICoefficientRepository { public CoefficientRepository(InvestmentContext context) : base(context) { } }
    public class SellRecommendationRepository : RepositoryEFCore<SellRecommendation>, ISellRecommendationRepository { public SellRecommendationRepository(InvestmentContext context) : base(context) { } }
    public class BuyRecommendationRepository : RepositoryEFCore<BuyRecommendation>, IBuyRecommendationRepository { public BuyRecommendationRepository(InvestmentContext context) : base(context) { } }
    public class AccountSummaryRepository : RepositoryEFCore<AccountSummary>, IAccountSummaryRepository { public AccountSummaryRepository(InvestmentContext context) : base(context) { } }
    public class CompanySummaryRepository : RepositoryEFCore<CompanySummary>, ICompanySummaryRepository { public CompanySummaryRepository(InvestmentContext context) : base(context) { } }
    public class ComissionSummaryRepository : RepositoryEFCore<ComissionSummary>, IComissionSummaryRepository { public ComissionSummaryRepository(InvestmentContext context) : base(context) { } }
    public class DividendSummaryRepository : RepositoryEFCore<DividendSummary>, IDividendSummaryRepository { public DividendSummaryRepository(InvestmentContext context) : base(context) { } }
    public class ExchangeRateSummaryRepository : RepositoryEFCore<ExchangeRateSummary>, IExchangeRateSummaryRepository { public ExchangeRateSummaryRepository(InvestmentContext context) : base(context) { } }
    // Common
    public class CurrencyRepository : RepositoryEFCore<Currency>, ICurrencyRepository { public CurrencyRepository(InvestmentContext context) : base(context) { } }
    #endregion
}
