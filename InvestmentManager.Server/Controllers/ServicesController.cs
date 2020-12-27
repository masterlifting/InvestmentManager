using InvestmentManager.BrokerService;
using InvestmentManager.Calculator;
using InvestmentManager.Entities.Market;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.Models;
using InvestmentManager.Models.Additional;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IInvestCalculator calculator;
        private readonly IInvestBrokerService brokerService;
        private readonly IInvestMapper mapper;
        private readonly ISummaryService summaryService;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IWebService webService;
        private readonly IPriceService priceService;
        private readonly IReportService reportService;
        private readonly IReckonerService reckonerService;

        public ServicesController(
            IUnitOfWorkFactory unitOfWork
            , IInvestCalculator calculator
            , IInvestBrokerService brokerService
            , IInvestMapper mapper
            , ISummaryService summaryService
            , UserManager<IdentityUser> userManager
            , IWebService webService
            , IPriceService priceService
            , IReportService reportService
            , IReckonerService reckonerService)
        {
            this.unitOfWork = unitOfWork;
            this.calculator = calculator;
            this.brokerService = brokerService;
            this.mapper = mapper;
            this.summaryService = summaryService;
            this.userManager = userManager;
            this.webService = webService;
            this.priceService = priceService;
            this.reportService = reportService;
            this.reckonerService = reckonerService;
        }

        [HttpGet("rate/")]
        public async Task<IActionResult> GetRate()
        {
            var response = await webService.GetCBRateAsync().ConfigureAwait(false);
            return response.IsSuccessStatusCode ? Ok(await response.Content.ReadFromJsonAsync<CBRF>().ConfigureAwait(false)) : NoContent();
        }

        [HttpPost("parsebrokerreports/"), Authorize]
        public async Task<IActionResult> ParseBcsReports()
        {
            var files = HttpContext.Request.Form.Files;
            string userId = userManager.GetUserId(User);
            try
            {
                var parsedReports = await brokerService.GetNewReportsAsync(files, userId).ConfigureAwait(false);
                return Ok(mapper.MapBcsReports(parsedReports));
            }
            catch
            {
                return NoContent();
            }
        }
        [HttpGet("parseprices/"), Authorize(Roles = "pestunov")]
        public async Task ParsePrices()
        {
            var newPricies = new List<Price>();
            var exchanges = unitOfWork.Exchange.GetAll();
            var tickers = unitOfWork.Ticker.GetPriceTikers();
            var priceConfigure = tickers.Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { TickerId = x.Id, Ticker = x.Name, y.ProviderName, y.ProviderUri });

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
            if (newPricies.Any())
            {
                await unitOfWork.Price.CreateEntitiesAsync(newPricies).ConfigureAwait(false);
                if (await unitOfWork.Price.CompletePostgresAsync().ConfigureAwait(false))
                {
                    var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);
                    await reckonerService.UpgradeByPriceChangeAsync(DataBaseType.Postgres, userIds).ConfigureAwait(false);
                }
            }
        }
        [HttpGet("parsereports/"), Authorize(Roles = "pestunov")]
        public async Task ParseReports()
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
            await unitOfWork.Report.CompletePostgresAsync().ConfigureAwait(false);
        }


        [HttpGet("resetcalculatordata/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ResetCalculatorData()
        {
            var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

            return await calculator.ResetCalculatorDataAsync(DataBaseType.Postgres, userIds).ConfigureAwait(false)
                ? Ok(new BaseActionResult { IsSuccess = true, Info = "Reset all calculator success." })
                : BadRequest(new BaseActionResult { IsSuccess = false, Info = "Reset all calculator failed!" });
        }
        [HttpGet("resetsummarydata/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ResetSummaryData()
        {
            var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

            return await summaryService.ResetSummaryDataAsync(DataBaseType.Postgres, userIds).ConfigureAwait(false)
                ? Ok(new BaseActionResult { IsSuccess = true, Info = "Reset all summary success." })
                : BadRequest(new BaseActionResult { IsSuccess = false, Info = "Reset all summary failed!" });
        }
    }
}
