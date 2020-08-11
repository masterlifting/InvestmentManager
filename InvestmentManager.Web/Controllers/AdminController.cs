using InvestmentManager.Calculator;
using InvestmentManager.Entities.Market;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    [Authorize(Roles = "pestunov")]
    public class AdminController : Controller
    {
        const string localUrl = "~/admin";
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IInvestmentCalculator calculator;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IPriceService priceService;
        private readonly IReportService reportService;

        public AdminController(
            IUnitOfWorkFactory unitOfWork
            , IInvestmentCalculator calculator
            , UserManager<IdentityUser> userManager
            , IWebHostEnvironment webHostEnvironment
            , IPriceService priceService
            , IReportService reportService)
        {
            this.unitOfWork = unitOfWork;
            this.calculator = calculator;
            this.userManager = userManager;
            this.webHostEnvironment = webHostEnvironment;
            this.priceService = priceService;
            this.reportService = reportService;
        }
        public IActionResult Index() => View();
        public async Task<IActionResult> UpdateAllCalculatorResult()
        {
            //pre delete all sql
            /*/
            unitOfWork.Rating.TruncateAndReseedSQL();
            unitOfWork.Coefficient.TruncateAndReseedSQL();
            unitOfWork.BuyRecommendation.TruncateAndReseedSQL();
            unitOfWork.SellRecommendation.TruncateAndReseedSQL();
            /*/
            //pre delete all Postgres
            unitOfWork.Rating.DeleteAndReseedPostgres();
            unitOfWork.Coefficient.DeleteAndReseedPostgres();
            unitOfWork.BuyRecommendation.DeleteAndReseedPostgres();
            unitOfWork.SellRecommendation.DeleteAndReseedPostgres();
            //*/
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            // add new
            unitOfWork.Coefficient.CreateEntities(await calculator.GetComplitedCoeffitientsAsync().ConfigureAwait(false));
            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            var ratings = await calculator.GetCompleatedRatingsAsync().ConfigureAwait(false);
            unitOfWork.Rating.CreateEntities(ratings);
            unitOfWork.SellRecommendation.CreateEntities(calculator.GetCompleatedSellRecommendations(userManager.Users, ratings));
            unitOfWork.BuyRecommendation.CreateEntities(calculator.GetCompleatedBuyRecommendations(ratings));

            await unitOfWork.CompleteAsync().ConfigureAwait(false);

            return LocalRedirect($"~/Home/{nameof(Index)}");
        }
        public async Task GetNewPrices()
        {
            var newPricies = new List<Price>();
            var exchanges = unitOfWork.Exchange.GetAll();
            var tickers = unitOfWork.Ticker.GetPriceTikers();
            var priceConfigure = tickers.Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { TickerId = x.Id, Ticker = x.Name, y.ProviderName, y.ProviderUri });

            int count = priceConfigure.Count();

            foreach (var i in priceConfigure)
            {
                try
                {
                    var newPrice = await priceService.GetPriceListAsync(i.ProviderName, i.TickerId, i.Ticker, i.ProviderUri).ConfigureAwait(false);
                    newPricies.AddRange(newPrice);
                    //($"Цены по {i.Ticker} загружены в количестве: {newPrice.Count} шт. Осталось компаний {--count}.");
                }
                catch (HttpRequestException ex)
                {
                    //($"На компании {i.Ticker} произошла ошибка {ex.Message}. Осталось компаний {--count}.");
                }
            }

            unitOfWork.Price.CreateEntities(newPricies);
            await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
        public async Task GetNewReports()
        {
            IDictionary<long, Report> lastReports = unitOfWork.Report.GetLastReports();
            int sourceCount = await unitOfWork.ReportSource.GetAll().CountAsync().ConfigureAwait(false);
            Task<List<Report>> newReportsTask = null;
            var reportsToSave = new List<Report>();
            await foreach (var i in unitOfWork.ReportSource.GetAll().AsAsyncEnumerable())
            {
                //($"\nОсталось проверить {sourceCount--} компаний.");
                //($"Беру последний отчет.");
                if (lastReports.ContainsKey(i.CompanyId))
                {
                    var lastReportDate = lastReports[i.CompanyId].DateReport;
                    //($"Проверяю, прошел ли квартал с момента последнего отчета у компании {i.Value}");
                    if (lastReportDate.AddDays(92) > DateTime.Now)
                    {
                        //($"У компании {i.Value} с момента последнего отчета еще не прошло 3 месяца.");
                        //($"Дата последнего отчета: {lastReportDate.ToShortDateString()}.");
                        continue;
                    }
                }

                List<Report> foundReports;
                try
                {
                    //($"Ищу новые отчеты по компании: {i.Value}");
                    newReportsTask = reportService.FindNewReportsAsync(i.CompanyId, i.Key, i.Value);
                    foundReports = await newReportsTask.ConfigureAwait(false);
                    newReportsTask = null;
                }
                catch
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    //Console.WriteLine($"Ошибка. Остановлено на {i.Value}");
                    //Console.WriteLine(newReportsTask.Exception.InnerException.Message);
                    //Console.ResetColor();
                    newReportsTask = null;
                    continue;
                }

                //Console.ForegroundColor = ConsoleColor.Cyan;
                //Console.WriteLine($"По компании {i.Value} добавлено: {foundReports.Count} новых отчетов");
                //Console.ResetColor();

                reportsToSave.AddRange(foundReports);
            }

            //("\nСохраняю все, что нашел...");
            unitOfWork.Report.CreateEntities(reportsToSave);
            await unitOfWork.CompleteAsync().ConfigureAwait(false);
        }
    }
}
