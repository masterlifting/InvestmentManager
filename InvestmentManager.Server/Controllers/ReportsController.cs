using InvestmentManager.Entities.Market;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly IConverterService converterService;

        public ReportsController(IUnitOfWorkFactory unitOfWork, IBaseRestMethod restMethod, IConverterService converterService)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.converterService = converterService;
        }

        [HttpGet("new/")]
        public async Task<List<ReportModel>> GetNew()
        {
            return await unitOfWork.Report.GetAll().Where(x => x.IsChecked == false).Select(z => new ReportModel
            {
                Id = z.Id,
                CompanyId = z.CompanyId,
                IsChecked = false,
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
            }).ToListAsync().ConfigureAwait(false);
        }

        [HttpGet("bycompanyid/{id}")]
        public async Task<List<ReportModel>> GetByCompanyId(long id)
        {
            var reports = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Reports;
            return reports is null ? null : reports.Select(x => new ReportModel
            {
                DateReport = x.DateReport,
                Quarter = converterService.ConvertToQuarter(x.DateReport.Month),
                Assets = x.Assets,
                CashFlow = x.CashFlow,
                Dividend = x.Dividends,
                GrossProfit = x.GrossProfit,
                LongTermDebt = x.LongTermDebt,
                NetProfit = x.NetProfit,
                Obligation = x.Obligations,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                Turnover = x.Turnover

            }).ToList();
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<ReportSummaryModel> GetSummaryByCompanyId(long id)
        {
            var reports = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Reports.OrderBy(x => x.DateReport);
            if (reports is null)
                return new ReportSummaryModel { IsHave = false };

            var dateLastReport = reports.Last().DateReport;
            return new ReportSummaryModel
            {
                IsHave = true,
                DateLastReport = dateLastReport,
                DateUpdate = reports.Last().DateUpdate,
                ReportsCount = reports.Count(),
                LastReportYear = dateLastReport.Year,
                LastReportQuarter = converterService.ConvertToQuarter(dateLastReport.Month)
            };
        }

        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, ReportModel model)
        {
            void UpdateReport(Report report)
            {
                report.IsChecked = true;
                report.DateUpdate = DateTime.Now;
                report.DateReport = model.DateReport;
                report.StockVolume = model.StockVolume;
                report.Revenue = model.Revenue;
                report.NetProfit = model.NetProfit;
                report.GrossProfit = model.GrossProfit;
                report.CashFlow = model.CashFlow;
                report.Assets = model.Assets;
                report.ShareCapital = model.ShareCapital;
                report.Dividends = model.Dividend;
                report.Turnover = model.Turnover;
                report.Obligations = model.Obligation;
                report.LongTermDebt = model.LongTermDebt;
            }
            var result = await restMethod.BasePutAsync<Report>(ModelState, id, UpdateReport);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await restMethod.BaseDeleteAsync<Report>(id).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
