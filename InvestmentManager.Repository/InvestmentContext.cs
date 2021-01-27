using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Calculate;
using InvestmentManager.Entities.Market;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InvestmentManager.Repository
{
    public class InvestmentContext : IdentityDbContext<IdentityUser>
    {
        #region DbSet
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountTransaction> AccountTransactions { get; set; }
        public DbSet<Comission> Comissions { get; set; }
        public DbSet<ComissionType> ComissionTypes { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<Isin> Isins { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<TransactionStatus> TransactionStatuses { get; set; }
        public DbSet<Coefficient> Coefficients { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<BuyRecommendation> BuyRecommendations { get; set; }
        public DbSet<SellRecommendation> SellRecommendations { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<ReportSource> ReportSources { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<Weekend> Weekends { get; set; }
        public DbSet<Industry> Industries { get; set; }
        public DbSet<Lot> Lots { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Ticker> Tickers { get; set; }
        #endregion
        #region Summary set
        public DbSet<CompanySummary> CompanySummaries { get; set; }
        public DbSet<AccountSummary> AccountSummaries { get; set; }
        public DbSet<DividendSummary> DividendSummaries { get; set; }
        public DbSet<ComissionSummary> ComissionSummaries { get; set; }
        public DbSet<ExchangeRateSummary> ExchangeRateSummaries { get; set; }
        #endregion

        public InvestmentContext(DbContextOptions<InvestmentContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<StockTransaction>()
                .HasOne(x => x.Ticker)
                .WithMany(x => x.StockTransactions)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
