using InvestmentManager.Calculator;
using InvestmentManager.Entities.Market;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels.ReportModels.CompanyReportModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentManager.ViewModels;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(Roles = "pestunov")]
    public class AdminController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IMemoryCache memoryCache;
        private readonly IInvestCalculator calculator;
        private readonly IPriceService priceService;
        private readonly IReportService reportService;
        private readonly UserManager<IdentityUser> userManager;

        public AdminController(
            IUnitOfWorkFactory unitOfWork
            , IMemoryCache memoryCache
            , IInvestCalculator calculator
            , IPriceService priceService
            , IReportService reportService
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.memoryCache = memoryCache;
            this.calculator = calculator;
            this.priceService = priceService;
            this.reportService = reportService;
            this.userManager = userManager;
        }

        [HttpGet("getreports")]
        public async Task<IEnumerable<CompanyReportForm>> GetReports()
        {
            var companies = new List<ViewModelBase>();
            if (memoryCache.TryGetValue("aCompanies", out List<ViewModelBase> companyModels))
                companies = companyModels;
            else
            {
                companies = await unitOfWork.Company.GetAll().Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("aCompanies", companies, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return unitOfWork.Report.GetAll()
                .Where(x => x.IsChecked == false)
                .AsEnumerable()
                .GroupBy(x => x.CompanyId)
                .Join(companies, x => x.Key, y => y.Id, (x, y) => new CompanyReportForm
                {
                    CompanyName = y.Name,
                    Reports = x.Select(z => new NewReportModel
                    {
                        ReportId = z.Id,
                        DateReport = z.DateReport,
                        Revenue = z.Revenue,
                        Assets = z.Assets,
                        CashFlow = z.CashFlow,
                        Dividend = z.Dividends,
                        GrossProfit = z.GrossProfit,
                        LongTermDebt = z.LongTermDebt,
                        NetProfit = z.NetProfit,
                        Obligation = z.Obligations,
                        ShareCapital = z.ShareCapital,
                        StockVolume = z.StockVolume,
                        Turnover = z.Turnover
                    }).ToList()
                });
        }
        [HttpPost("deletereport")]
        public async Task<IActionResult> DeleteReport([FromBody] long id)
        {
            var deletedReport = await unitOfWork.Report.FindByIdAsync(id).ConfigureAwait(false);
            if (deletedReport != null)
            {
                try
                {
                    unitOfWork.Report.DeleteEntity(deletedReport);
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
        [HttpPost("savereport")]
        public async Task<IActionResult> SaveReport([FromBody] NewReportModel report)
        {
            var savedReport = await unitOfWork.Report.FindByIdAsync(report.ReportId).ConfigureAwait(false);
            if (savedReport != null)
            {
                try
                {
                    savedReport.IsChecked = true;
                    savedReport.DateUpdate = DateTime.Now;
                    savedReport.DateReport = report.DateReport;
                    savedReport.StockVolume = report.StockVolume;
                    savedReport.Revenue = report.Revenue;
                    savedReport.NetProfit = report.NetProfit;
                    savedReport.GrossProfit = report.GrossProfit;
                    savedReport.CashFlow = report.CashFlow;
                    savedReport.Assets = report.Assets;
                    savedReport.ShareCapital = report.ShareCapital;
                    savedReport.Dividends = report.Dividend;
                    savedReport.Turnover = report.Turnover;
                    savedReport.Obligations = report.Obligation;
                    savedReport.LongTermDebt = report.LongTermDebt;
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
        [HttpGet("recalculateall")]
        public async Task<IActionResult> RecalculateAll()
        {
            try
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
                await unitOfWork.Coefficient.CreateEntitiesAsync(await calculator.GetComplitedCoeffitientsAsync().ConfigureAwait(false)).ConfigureAwait(false);
                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                var ratings = await calculator.GetCompleatedRatingsAsync().ConfigureAwait(false);
                await unitOfWork.Rating.CreateEntitiesAsync(ratings).ConfigureAwait(false);
                var sellRecommendations = calculator.GetCompleatedSellRecommendations(userManager.Users, ratings);
                await unitOfWork.SellRecommendation.CreateEntitiesAsync(sellRecommendations).ConfigureAwait(false);
                await unitOfWork.BuyRecommendation.CreateEntitiesAsync(calculator.GetCompleatedBuyRecommendations(ratings)).ConfigureAwait(false);

                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        [HttpGet("getnewprices")]
        public async Task<IActionResult> GetNewPrices()
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
                }
                catch
                {
                    continue;
                }
            }

            await unitOfWork.Price.CreateEntitiesAsync(newPricies).ConfigureAwait(false);
            try
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                unitOfWork.Price.PostgresAutoReseed();
            }
            finally
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return Ok();
        }
        [HttpGet("getnewreports")]
        public async Task<IActionResult> GetNewReports()
        {
            IDictionary<long, Report> lastReports = unitOfWork.Report.GetLastReports();
            var reportSource = await unitOfWork.ReportSource.GetAll().ToListAsync().ConfigureAwait(false);
            var reportsToSave = new List<Report>();
            foreach (var i in reportSource)
            {
                if (lastReports.ContainsKey(i.CompanyId))
                {
                    var lastReportDate = lastReports[i.CompanyId].DateReport;
                    if (lastReportDate.AddDays(92) > DateTime.Now)
                        continue;
                }

                List<Report> foundReports;
                try
                {
                    foundReports = await reportService.FindNewReportsAsync(i.CompanyId, i.Key, i.Value).ConfigureAwait(false);
                }
                catch
                {
                    continue;
                }

                reportsToSave.AddRange(foundReports);
            }

            await unitOfWork.Report.CreateEntitiesAsync(reportsToSave);
            try
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                unitOfWork.Report.PostgresAutoReseed();
            }
            finally
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return Ok();
        }
    }
}


