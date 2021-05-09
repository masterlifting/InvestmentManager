using InvestmentManager.BrokerService;
using InvestmentManager.Calculator;
using InvestmentManager.Calculator.ConfigurationBinding;
using InvestmentManager.Mapper.Implimentations;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.ReportFinder.Implimentations;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Server.JwtService;
using InvestmentManager.Server.RestServices;
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
        readonly string ReactOrigins = "reactOrigins";
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.Bind("CalculatedWeight", new WeightConfig());
            Configuration.Bind("SellRecommendation", new SellRecommendationConfig());
            Configuration.Bind("BuyRecommendation", new BuyRecommendationConfig());

            services.AddDbContext<InvestmentContext>(provider =>
            {
                provider.UseLazyLoadingProxies();
                /*/
                 provider.UseNpgsql(Configuration["ConnectionStrings:LocalPostgresConnection"]);
                 /*/
                provider.UseNpgsql(Configuration["ConnectionStrings:PostgresConnection"]);
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

            services.AddCors(options =>
            {
                options.AddPolicy(ReactOrigins, policy =>
                {
                    policy.WithOrigins("https://paviams.com", "https://react.paviams.com");
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                    policy.AllowCredentials();
                });
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JwtIssuer"],
                    ValidAudience = Configuration["JwtAudience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"]))
                };
            });

            services.AddResponseCompression(x => x.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" }));
            services.AddRazorPages();
            services.AddHttpClient<IWebService, WebService>(x =>
            {
                x.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:80.0) Gecko/20100101 Firefox/80.0");
                x.DefaultRequestHeaders.Add("Accept", "application/json, text/html, */*; q=0.01");
                x.DefaultRequestHeaders.Add("Connection", "keep-alive");
            });

            services.AddScoped<IBaseRestMethod, BaseRestMethod>();
            #region Unit of work factory
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountSummaryRepository, AccountSummaryRepository>();
            services.AddScoped<IAccountTransactionRepository, AccountTransactionRepository>();
            services.AddScoped<IComissionRepository, ComissionRepository>();
            services.AddScoped<IComissionSummaryRepository, ComissionSummaryRepository>();
            services.AddScoped<IComissionTypeRepository, ComissionTypeRepository>();
            services.AddScoped<IDividendRepository, DividendRepository>();
            services.AddScoped<IDividendSummaryRepository, DividendSummaryRepository>();
            services.AddScoped<IIsinRepository, IsinRepository>();
            services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
            services.AddScoped<IExchangeRateSummaryRepository, ExchangeRateSummaryRepository>();
            services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
            services.AddScoped<ITransactionStatusRepository, TransactionStatusRepository>();
            services.AddScoped<IPriceRepository, PriceRepository>();
            services.AddScoped<ILotRepository, LotRepository>();
            services.AddScoped<ITickerRepository, TickerRepository>();
            services.AddScoped<ISectorRepository, SectorRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanySummaryRepository, CompanySummaryRepository>();
            services.AddScoped<IExchangeRepository, ExchangeRepository>();
            services.AddScoped<IWeekendRepository, WeekendRepository>();
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
            services.AddSingleton<ICatalogService, CatalogService>();

            services.AddScoped<IReckonerService, ReckonerService>();
            services.AddScoped<IInvestBrokerService, InvestBrokerService>();
            services.AddScoped<IPriceService, PriceService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IInvestCalculator, InvestCalculator>();
            services.AddScoped<IIOService, IOService>();
            services.AddScoped<IConverterService, ConverterService>();
            services.AddScoped<ISummaryService, SummaryService>();
            services.AddScoped<IInvestMapper, InvestMapper>();
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors(ReactOrigins);

            app.UseResponseCompression();

            app.UseMiddleware<JwtMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
