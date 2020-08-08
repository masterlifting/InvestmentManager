using InvestmentManager.BrokerService;
using InvestmentManager.Calculator;
using InvestmentManager.Calculator.ConfigurationBinding;
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

            services.AddDbContext<InvestmentContext>(x => x.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
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

            #region Custom Service
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
            services.AddScoped<IInvestmentCalculator, InvestmentCalculator>();

            services.AddScoped<ICustomBrokerService, CustomBrokerService>();
            services.AddScoped<IFinancialAgregator, FinancialAgregator>();
            services.AddScoped<IPortfolioAgregator, PortfolioAgregator>();

            services.AddSingleton<IConverterService, ConverterService>();
            services.AddScoped<ILoaderService, LoaderService>();
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
