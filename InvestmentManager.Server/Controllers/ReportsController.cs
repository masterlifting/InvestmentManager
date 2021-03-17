using InvestmentManager.Entities.Market;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly IConverterService converterService;
        private readonly IReckonerService reckonerService;
        private readonly UserManager<IdentityUser> userManager;

        public ReportsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , IConverterService converterService
            , IReckonerService reckonerService
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.converterService = converterService;
            this.reckonerService = reckonerService;
            this.userManager = userManager;
        }


        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var reports = (await unitOfWork.Company.FindByIdAsync(id))?.Reports;
            return reports is null
                ? NoContent()
                : Ok(reports.Select(x => new ReportModel
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

                }).ToList());
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            var reports = (await unitOfWork.Company.FindByIdAsync(id))?.Reports.OrderBy(x => x.DateReport);

            return reports is null || !reports.Any() ? NoContent() : Ok(new SummaryReport
            {
                DateLastReport = reports.Last().DateReport,
                DateUpdate = reports.Last().DateUpdate,
                ReportsCount = reports.Count(),
                LastReportYear = reports.Last().DateReport.Year,
                LastReportQuarter = converterService.ConvertToQuarter(reports.Last().DateReport.Month)
            });
        }

        [HttpGet("new/"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> GetNew()
        {
            return Ok(await unitOfWork.Report.GetAll().Where(x => x.IsChecked == false).Select(x => new ReportModel
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                IsChecked = false,
                DateReport = x.DateReport,
                Revenue = x.Revenue,
                Assets = x.Assets,
                CashFlow = x.CashFlow,
                Dividend = x.Dividends,
                GrossProfit = x.GrossProfit,
                LongTermDebt = x.LongTermDebt,
                NetProfit = x.NetProfit,
                Obligation = x.Obligations,
                ShareCapital = x.ShareCapital,
                StockVolume = x.StockVolume,
                Turnover = x.Turnover
            }).ToListAsync());
        }

        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, ReportModel model)
        {
            void UpdateReport(Report report)
            {
                report.DateUpdate = DateTime.Now;
                report.IsChecked = model.IsChecked;
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

            if (result.IsSuccess)
            {
                long companyId = (await unitOfWork.Report.FindByIdAsync(id)).CompanyId;
                var userIds = await userManager.Users.Select(x => x.Id).ToArrayAsync();
                result.Info += await reckonerService.UpgradeByReportChangeAsync(DataBaseType.Postgres, companyId, userIds) ? " Recalculated" : " NOT Recalculated.";
                return Ok(result);
            }
            else
                return BadRequest(result);
        }

        [HttpDelete("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await restMethod.BaseDeleteAsync<Report>(id);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
