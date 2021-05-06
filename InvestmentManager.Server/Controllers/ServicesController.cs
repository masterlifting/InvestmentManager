using InvestmentManager.BrokerService;
using InvestmentManager.Calculator;
using InvestmentManager.Entities.Market;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.Models;
using InvestmentManager.Models.Additional;
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
        public async Task<CBRF> GetRate()
        {
            CBRF result = new();

            var response = await webService.GetCBRateAsync();

            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadFromJsonAsync<CBRF>();

            return result;
        }

        [HttpPost("parsebrokerreports/"), Authorize]
        public async Task<IActionResult> ParseBcsReports()
        {
            var files = HttpContext.Request.Form.Files;
            string userId = userManager.GetUserId(User);
            try
            {
                var parsedReports = await brokerService.GetNewReportsAsync(files, userId);
                return Ok(mapper.MapBcsReports(parsedReports));
            }
            catch
            {
                return NoContent();
            }
        }
        [HttpGet("parseprices/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ParsePrices()
        {
            var companyCountWithParsedPrices = await priceService.DownloadNewStockPricesAsync(365);

            string recalculatedResult = "";
            if (companyCountWithParsedPrices > 0)
            {
                var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync();
                bool isRecalculated = await reckonerService.UpgradeByPriceChangeAsync(DataBaseType.Postgres, userIds);
                recalculatedResult = isRecalculated ? "Recalculated" : "NOT Recalculated";
            }

            return companyCountWithParsedPrices >= 0
                            ? Ok(new BaseActionResult { IsSuccess = true, Info = $"Company: {companyCountWithParsedPrices}. {recalculatedResult}" })
                            : BadRequest(new BaseActionResult { IsSuccess = false, Info = "Prices update failed!" });
        }

        [HttpGet("parsereports/"), Authorize(Roles = "pestunov")]
        public async Task ParseReports()
        {
            IDictionary<long, Report> lastReports = await unitOfWork.Report.GetLastReportsAsync();
            var reportSources = await unitOfWork.ReportSource.GetAll().ToListAsync();

            foreach (var i in reportSources)
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
                    foundReports = await reportService.FindNewReportsAsync(i.CompanyId, i.Key, i.Value);
                }
                catch
                {
                    continue;
                }

                if (foundReports.Any())
                {
                    await unitOfWork.Report.CreateEntitiesAsync(foundReports);
                    await unitOfWork.Report.CompletePostgresAsync();
                }

                await Task.Delay(5000);
            }
        }


        [HttpGet("resetcalculatordata/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ResetCalculatorData()
        {
            var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync();

            /*/
             DropCalculations();
             return Ok(new BaseActionResult { IsSuccess = true, Info = "Is Drop!" });
             /*/
            return await calculator.ResetCalculatorDataAsync(DataBaseType.Postgres, userIds)
                ? Ok(new BaseActionResult { IsSuccess = true, Info = "Reset all calculator success." })
                : BadRequest(new BaseActionResult { IsSuccess = false, Info = "Reset all calculator failed!" });
            //*/
        }
        [HttpGet("resetsummarydata/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ResetSummaryData()
        {
            var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync();

            return await summaryService.ResetSummaryDataAsync(DataBaseType.Postgres, userIds)
                ? Ok(new BaseActionResult { IsSuccess = true, Info = "Reset all summary success." })
                : BadRequest(new BaseActionResult { IsSuccess = false, Info = "Reset all summary failed!" });
        }

        void DropCalculations()
        {
            unitOfWork.AccountTransaction.DeleteAndReseedPostgres();
            unitOfWork.StockTransaction.DeleteAndReseedPostgres();
            unitOfWork.Dividend.DeleteAndReseedPostgres();
            unitOfWork.Comission.DeleteAndReseedPostgres();
            unitOfWork.ExchangeRate.DeleteAndReseedPostgres();

            unitOfWork.AccountSummary.DeleteAndReseedPostgres();
            unitOfWork.CompanySummary.DeleteAndReseedPostgres();
            unitOfWork.DividendSummary.DeleteAndReseedPostgres();
            unitOfWork.ComissionSummary.DeleteAndReseedPostgres();
            unitOfWork.ExchangeRateSummary.DeleteAndReseedPostgres();

            unitOfWork.SellRecommendation.DeleteAndReseedPostgres();
        }
    }
}
