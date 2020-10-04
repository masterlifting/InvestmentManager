using InvestmentManager.BrokerService;
using InvestmentManager.Calculator;
using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Mapper.Implimentations;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.PriceFinder.Implimentations;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.ReportFinder.Implimentations;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Services.Implimentations;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;

namespace InvestmentManager.Server
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

            services.AddDbContext<InvestmentContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                /*/
                provider.UseNpgsql(configuration["ConnectionStrings:LocalPostgresConnection"]);
                /*/
                provider.UseNpgsql(configuration["ConnectionStrings:PostgresConnection"]);
                //*/
            });
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;

            }).AddEntityFrameworkStores<InvestmentContext>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JwtIssuer"],
                    ValidAudience = configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]))
                };
            });

            services.AddResponseCompression(options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
            services.AddControllers();
            services.AddRazorPages();

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
            services.AddScoped<IInvestBrokerService, InvestBrokerService>();
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
            #region Mapper
            services.AddScoped<IPortfolioMapper, PortfolioMapper>();
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseResponseCompression();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
