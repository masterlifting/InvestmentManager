using InvestmentManager.BrokerService;
using InvestmentManager.BrokerService.Implimentations;
using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.Calculator;
using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.PriceFinder.Implimentations;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.ReportFinder.Implimentations;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Service.Implimentations;
using InvestmentManager.Service.Interfaces;
using InvestmentManager.Web.ViewAgregator.Implimentations;
using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InvestmentManager.Web
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IConfiguration configuration) => this.configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            configuration.Bind("CalculatedWeight", new WeightConfig());
            configuration.Bind("SellRecommendation", new SellRecommendationConfig());
            configuration.Bind("BuyRecommendation", new BuyRecommendationConfig());

            services.AddDbContext<InvestmentContext>(x => x.UseNpgsql(configuration["Connection:PostgresTestConnection"]));
            services.AddIdentity<IdentityUser, IdentityRole>(configuration =>
            {
                configuration.Password.RequiredLength = 10;
                configuration.Password.RequireDigit = true;
                configuration.Password.RequireLowercase = true;
                configuration.Password.RequireUppercase = true;
                configuration.Password.RequireNonAlphanumeric = false;
                configuration.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<InvestmentContext>();

            services.AddControllersWithViews();

            #region Unit of work factory
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountTransactionRepository, AccountTransactionRepository>();
            services.AddScoped<IComissionRepository, ComissionRepository>();
            services.AddScoped<IComissionTypeRepository, ComissionTypeRepository>();
            services.AddScoped<IDividendRepository, DividendRepository>();
            services.AddScoped<IIsinRepository, IsinRepository>();
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
            services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
            services.AddScoped<IPriceRepository, PriceRepository>();
            services.AddScoped<ILotRepository, LotRepository>();
            services.AddScoped<ITickerRepository, TickerRepository>();
            services.AddScoped<ISectorRepository, SectorRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IExchangeRepository, ExchangeRepository>();
            services.AddScoped<IIndustryRepository, IndustryRepository>();
            services.AddScoped<IReportSourceRepository, ReportSourceRepository>();
            services.AddScoped<IRatingRepository, RatingRepository>();
            services.AddScoped<ICoefficientRepository, CoefficientRepository>();
            services.AddScoped<ISellRecommendationRepository, SellRecommendationRepository>();
            services.AddScoped<IBuyRecommendationRepository, BuyRecommendationRepository>();
            services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
            #endregion
            
            #region Service
            #region Broker Service
            services.AddScoped<IBcsParser, BcsParser>();
            services.AddScoped<IReportMapper, ReportMapper>();
            services.AddScoped<IReportFilter, ReportFilter>();
            services.AddScoped<IInvestBrokerService, InvestBrokerService>();
            #endregion
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IInvestCalculator, InvestCalculator>();

            services.AddHttpClient<IWebService, WebService>(x =>
            {
                x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
                x.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*; q=0.01");
                x.DefaultRequestHeaders.Add("Connection", "keep-alive");
            });
            services.AddScoped<IIOService, IOService>();
            services.AddScoped<IConverterService, ConverterService>();
            #endregion
            
            #region View agregator
            services.AddScoped<IFinancialAgregator, FinancialAgregator>();
            services.AddScoped<IPortfolioAgregator, PortfolioAgregator>();
            #endregion

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}
