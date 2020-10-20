using System.Collections.Generic;
using System.Linq;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels.ReportHistory;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;

        public ReportController(IUnitOfWorkFactory unitOfWork, IConverterService converterService)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
        }
        [HttpGet("shorthistory")]
        public CompanyReportHistoryShortModel GetShortHistory(long id)
        {
            var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id).OrderBy(x => x.DateReport);
            var dateLastReport = reports.Last().DateReport;
            return new CompanyReportHistoryShortModel
            {
                DateLastReport = dateLastReport.ToShortDateString(),
                DateUpdate = reports.Last().DateUpdate.ToShortDateString(),
                ReportCount = $"{reports.Count()}",
                LastYear = dateLastReport.Year.ToString(),
                LastQuarter = converterService.ConvertToQuarter(dateLastReport.Month).ToString()
            };
        }
        [HttpGet("history")]
        public List<CompanyReportHistoryModel> GetHistory(long id)
        {
            var reports = unitOfWork.Report.GetAll().Where(x => x.CompanyId == id).OrderByDescending(x => x.DateReport);
            var result = new List<CompanyReportHistoryModel>();
            foreach (var report in reports)
            {
                result.Add(new CompanyReportHistoryModel
                {
                    DateReport = report.DateReport.ToShortDateString(),
                    Quarter = converterService.ConvertToQuarter(report.DateReport.Month).ToString(),
                    Assets = report.Assets.ToString("f0"),
                    CashFlow = report.CashFlow.ToString("f0"),
                    Dividend = report.Dividends.ToString("f2"),
                    GrossProfit = report.GrossProfit.ToString("f0"),
                    LongTermDebt = report.LongTermDebt.ToString("f0"),
                    NetProfit = report.NetProfit.ToString("f0"),
                    Obligation = report.Obligations.ToString("f0"),
                    Revenue = report.Revenue.ToString("f0"),
                    ShareCapital = report.ShareCapital.ToString("f0"),
                    Turnover = report.Turnover.ToString("f0")
                });
            }

            return result;
        }

    }
}
