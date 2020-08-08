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

        public void CreateEntity(TEntity entity) => context.Set<TEntity>().AddAsync(entity);
        public void CreateEntities(IEnumerable<TEntity> entities) => context.Set<TEntity>().AddRangeAsync(entities);

        public void UpdateEntity(TEntity entity) => context.Set<TEntity>().Update(entity);
        public void UpdateEntities(IEnumerable<TEntity> entities) => context.Set<TEntity>().UpdateRange(entities);

        public void DeleteEntity(TEntity entity) => context.Set<TEntity>().Remove(entity);
        public void DeleteEntities(IEnumerable<TEntity> entities) => context.Set<TEntity>().RemoveRange(entities);
        public void TruncateAndReseed()
        {
            var tableName = context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            context.Database.ExecuteSqlRaw($"TRUNCATE TABLE {tableName}");
            context.Database.ExecuteSqlRaw($"DBCC CHECKIDENT('{tableName}', RESEED, 1)");
        }
    }
    public enum OrderType
    {
        OrderBy,
        OrderByDesc
    }

    #region Repository Classes
    // Broker
    class AccountRepository : RepositoryEFCore<Account>, IAccountRepository { public AccountRepository(InvestmentContext context) : base(context) { } }
    class AccountTransactionRepository : RepositoryEFCore<AccountTransaction>, IAccountTransactionRepository { public AccountTransactionRepository(InvestmentContext context) : base(context) { } }
    class ComissionRepository : RepositoryEFCore<Comission>, IComissionRepository { public ComissionRepository(InvestmentContext context) : base(context) { } }
    class ComissionTypeRepository : RepositoryEFCore<ComissionType>, IComissionTypeRepository { public ComissionTypeRepository(InvestmentContext context) : base(context) { } }
    class DividendRepository : RepositoryEFCore<Dividend>, IDividendRepository { public DividendRepository(InvestmentContext context) : base(context) { } }
    class ExchangeRateRepository : RepositoryEFCore<ExchangeRate>, IExchangeRateRepository { public ExchangeRateRepository(InvestmentContext context) : base(context) { } }
    class StockTransactionRepository : RepositoryEFCore<StockTransaction>, IStockTransactionRepository { public StockTransactionRepository(InvestmentContext context) : base(context) { } }
    class TransactionStatusRepository : RepositoryEFCore<TransactionStatus>, ITransactionStatusRepository { public TransactionStatusRepository(InvestmentContext context) : base(context) { } }

    // Market
    class ExchangeRepository : RepositoryEFCore<Exchange>, IExchangeRepository { public ExchangeRepository(InvestmentContext context) : base(context) { } }
    class IsinRepository : RepositoryEFCore<Isin>, IIsinRepository { public IsinRepository(InvestmentContext context) : base(context) { } }
    class TickerRepository : RepositoryEFCore<Ticker>, ITickerRepository
    {
        private readonly InvestmentContext context;
        public TickerRepository(InvestmentContext context) : base(context) => this.context = context;

        public IQueryable<Ticker> GetTickerIncludedLot() => context.Tickers.Include(x => x.Lot);

        public List<Ticker> GetUniqueTikersForPrice() =>
            context.Tickers.AsEnumerable().GroupBy(x => x.CompanyId).Select(x => x.FirstOrDefault()).ToList();
    }
    class LotRepository : RepositoryEFCore<Lot>, ILotRepository { public LotRepository(InvestmentContext context) : base(context) { } }
    class CompanyRepository : RepositoryEFCore<Company>, ICompanyRepository { public CompanyRepository(InvestmentContext context) : base(context) { } }
    class IndustryRepository : RepositoryEFCore<Industry>, IIndustryRepository { public IndustryRepository(InvestmentContext context) : base(context) { } }
    class SectorRepository : RepositoryEFCore<Sector>, ISectorRepository { public SectorRepository(InvestmentContext context) : base(context) { } }
    class ReportSourceRepository : RepositoryEFCore<ReportSource>, IReportSourceRepository { public ReportSourceRepository(InvestmentContext context) : base(context) { } }
    class ReportRepository : RepositoryEFCore<Report>, IReportRepository
    {
        private readonly InvestmentContext context;
        public ReportRepository(InvestmentContext context) : base(context) => this.context = context;

        public IDictionary<long, Report> GetAllLastReportsGroupById()
        {
            var result = new Dictionary<long, Report>();

            var lastReports = context.Reports.AsNoTracking().AsEnumerable()
                .GroupBy(x => x.CompanyId).Select(x => new { Id = x.Key, Report = x.OrderBy(x => x.DateReport).LastOrDefault() });

            foreach (var item in lastReports)
            {
                result.Add(item.Id, item.Report);
            }

            return result;
        }
        public async Task<List<DateTime>> GetFourLastReportDateAsync(long companyId) =>
            await context.Reports.AsNoTracking()
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.DateReport.Date)
            .Select(x => x.DateReport.Date)
            .Take(4)
            .ToListAsync()
            .ConfigureAwait(false);
    }
    class PriceRepository : RepositoryEFCore<Price>, IPriceRepository
    {
        private readonly InvestmentContext context;
        public PriceRepository(InvestmentContext context) : base(context) => this.context = context;

        public Dictionary<long, IEnumerable<Price>> GetGroupedSortedDescPrices()
        {
            var result = new Dictionary<long, IEnumerable<Price>>();

            foreach (var i in context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices))
            {
                long companyId = i.Id;
                var prices = i.Tickers.FirstOrDefault().Prices;

                if (prices.Any())
                    result.Add(companyId, prices.OrderBy(x => x.BidDate.Date).TakeLast(230));
            }

            return result;
        }
        public Dictionary<long, decimal> GetLastPrices()
        {
            var result = new Dictionary<long, decimal>();

            foreach (var i in context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices))
            {
                long companyId = i.Id;
                var price = i.Tickers.FirstOrDefault().Prices.Where(x => x.BidDate.Date > DateTime.Now.AddDays(-15)).OrderBy(x => x.BidDate.Date).LastOrDefault();

                if (price != null)
                    result.Add(companyId, price.Value);
                else
                    result.Add(companyId, 0);
            }

            return result;
        }
        public async Task<IEnumerable<Price>> GetSortedPricesByDateAsync(long companyId, OrderType order) => order == OrderType.OrderByDesc
                ? (await context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
                                .Tickers.FirstOrDefault().Prices.OrderByDescending(x => x.BidDate.Date).TakeLast(230)
                : (await context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
                                .Tickers.FirstOrDefault().Prices.OrderBy(x => x.BidDate.Date).TakeLast(230);
        public async Task<List<DateTime>> GetSortedPriceDateAsync(long tickerId, int countElement) =>
            await context.Prices.Where(x => x.TickerId == tickerId)
                .OrderByDescending(x => x.BidDate.Date).Select(x => x.BidDate.Date).Take(countElement)
                .ToListAsync().ConfigureAwait(false);

        public async Task<decimal> GetPriceByDateReportAsync(long companyId, DateTime dateReport)
        {
            var prices = (await context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices)
                .FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
                .Tickers.FirstOrDefault().Prices
                .Where(x => x.BidDate.Date <= dateReport.Date);

            return prices.Any() ? prices.OrderByDescending(x => x.BidDate.Date).FirstOrDefault().Value : 0;
        }

        public int GetCompanyCountWithPrices() => context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices).Select(x => x.Tickers.FirstOrDefault()).Where(x => x.Prices.Any()).Count();
    }
    // Calculate
    class RatingRepository : RepositoryEFCore<Rating>, IRatingRepository { public RatingRepository(InvestmentContext context) : base(context) { } }
    class CoefficientRepository : RepositoryEFCore<Coefficient>, ICoefficientRepository
    {
        private readonly InvestmentContext context;
        public CoefficientRepository(InvestmentContext context) : base(context) => this.context = context;

        public async Task<List<(decimal, Report)>> GetSortedCoefficientCalculatingDataAsync(long companyId)
        {
            var result = new List<(decimal, Report)>();

            foreach (var i in (await context.Companies.Include(x => x.Reports).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false)).Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport.Date))
            {
                var prices = (await context.Companies.Include(x => x.Tickers).ThenInclude(x => x.Prices).FirstOrDefaultAsync(x => x.Id == i.CompanyId).ConfigureAwait(false))
                    .Tickers.FirstOrDefault().Prices.Where(x => x.BidDate.Date <= i.DateReport.Date);

                result.Add((prices.Any() ? prices.OrderByDescending(x => x.BidDate.Date).FirstOrDefault().Value : 0, i));
            }

            return result;
        }
        public Dictionary<string, List<(DateTime, Coefficient)>> GetViewData()
        {
            var result = new Dictionary<string, List<(DateTime, Coefficient)>>();

            foreach (var i in context.Companies.Include(x => x.Reports).ThenInclude(x => x.Coefficient))
            {
                var temp = new List<(DateTime, Coefficient)>();

                foreach (var j in i.Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport.Date))
                {
                    temp.Add((j.DateReport, j.Coefficient));
                }

                result.Add(i.Name, temp);
            }

            return result;
        }
        public async Task<List<Coefficient>> GetSortedCoefficientsAsync(long companyId) =>
            (await context.Companies.Include(x => x.Reports).ThenInclude(x => x.Coefficient).FirstOrDefaultAsync(x => x.Id == companyId).ConfigureAwait(false))
            .Reports.Where(x => x.IsChecked == true).OrderBy(x => x.DateReport.Date).Select(x => x.Coefficient).ToList();
    }
    class SellRecommendationRepository : RepositoryEFCore<SellRecommendation>, ISellRecommendationRepository { public SellRecommendationRepository(InvestmentContext context) : base(context) { } }
    class BuyRecommendationRepository : RepositoryEFCore<BuyRecommendation>, IBuyRecommendationRepository { public BuyRecommendationRepository(InvestmentContext context) : base(context) { } }
    // Common
    class CurrencyRepository : RepositoryEFCore<Currency>, ICurrencyRepository { public CurrencyRepository(InvestmentContext context) : base(context) { } }
    #endregion
}
