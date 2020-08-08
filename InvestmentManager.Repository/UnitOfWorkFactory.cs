using InvestmentManager.Entities.Basic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Repository
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly InvestmentContext context;
        public UnitOfWorkFactory(InvestmentContext context) => this.context = context;

        // Broker
        public IAccountRepository Account => new AccountRepository(context);
        public IDividendRepository Dividend => new DividendRepository(context);
        public IIsinRepository Isin => new IsinRepository(context);
        public IComissionRepository Comission => new ComissionRepository(context);
        public IExchangeRateRepository ExchangeRate => new ExchangeRateRepository(context);
        public IComissionTypeRepository ComissionType => new ComissionTypeRepository(context);
        public IStockTransactionRepository StockTransaction => new StockTransactionRepository(context);
        public ITransactionStatusRepository TransactionStatus => new TransactionStatusRepository(context);
        public IAccountTransactionRepository AccountTransaction => new AccountTransactionRepository(context);

        // Market
        public IPriceRepository Price => new PriceRepository(context);
        public ILotRepository Lot => new LotRepository(context);
        public ITickerRepository Ticker => new TickerRepository(context);
        public ISectorRepository Sector => new SectorRepository(context);
        public IReportRepository Report => new ReportRepository(context);
        public ICompanyRepository Company => new CompanyRepository(context);
        public IExchangeRepository Exchange => new ExchangeRepository(context);
        public IIndustryRepository Industry => new IndustryRepository(context);
        public IReportSourceRepository ReportSource => new ReportSourceRepository(context);

        // Calculate
        public IRatingRepository Rating => new RatingRepository(context);
        public ICoefficientRepository Coefficient => new CoefficientRepository(context);
        public ISellRecommendationRepository SellRecommendation => new SellRecommendationRepository(context);
        public IBuyRecommendationRepository BuyRecommendation => new BuyRecommendationRepository(context);

        // Common
        public ICurrencyRepository Currency => new CurrencyRepository(context);


        public async Task<int> CompleteAsync() => await context.SaveChangesAsync().ConfigureAwait(false);
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
