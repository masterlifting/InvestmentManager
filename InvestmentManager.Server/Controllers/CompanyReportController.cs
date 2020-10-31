using InvestmentManager.Entities.Market;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ReportModels.CompanyReportModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyReportController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;
        private readonly IReportService reportService;
        private readonly IMemoryCache memoryCache;

        public CompanyReportController(
            IUnitOfWorkFactory unitOfWork
            , IConverterService converterService
            , IReportService reportService
            , IMemoryCache memoryCache)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
            this.reportService = reportService;
            this.memoryCache = memoryCache;
        }
        [HttpGet("short")]
        public CompanyReportShortModel GetShortHistory(long id)
        {
            var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id).OrderBy(x => x.DateReport);
            var dateLastReport = reports.Last().DateReport;
            return new CompanyReportShortModel
            {
                DateLastReport = dateLastReport.ToShortDateString(),
                DateUpdate = reports.Last().DateUpdate.ToShortDateString(),
                ReportCount = $"{reports.Count()}",
                LastYear = dateLastReport.Year.ToString(),
                LastQuarter = converterService.ConvertToQuarter(dateLastReport.Month).ToString()
            };
        }
        [HttpGet("full")]
        public List<CompanyReportFullModel> GetFullHistory(long id)
        {
            var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id).OrderByDescending(x => x.DateReport);
            var result = new List<CompanyReportFullModel>();
            foreach (var report in reports)
            {
                result.Add(new CompanyReportFullModel
                {
                    DateReport = report.DateReport,
                    Quarter = converterService.ConvertToQuarter(report.DateReport.Month),
                    Assets = report.Assets,
                    CashFlow = report.CashFlow,
                    Dividend = report.Dividends,
                    GrossProfit = report.GrossProfit,
                    LongTermDebt = report.LongTermDebt,
                    NetProfit = report.NetProfit,
                    Obligation = report.Obligations,
                    Revenue = report.Revenue,
                    ShareCapital = report.ShareCapital,
                    Turnover = report.Turnover
                });
            }

            return result;
        }


        [HttpGet("parse"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ParseReports()
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
        [HttpGet("checknew"), Authorize(Roles = "pestunov")]
        public async Task<IEnumerable<CompanyReportForm>> CheckNewReports()
        {
            if (!memoryCache.TryGetValue("companies", out List<ViewModelBase> companies))
            {
                companies = await unitOfWork.Company.GetAll().Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("companies", companies, TimeSpan.FromSeconds(30));
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
        [HttpGet("deletechecked"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> DeleteChecked(long id)
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
        [HttpPost("savechecked"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SaveChecked([FromBody] NewReportModel report)
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
    }
}
