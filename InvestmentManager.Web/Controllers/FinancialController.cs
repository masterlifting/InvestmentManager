using InvestmentManager.Calculator;
using InvestmentManager.Repository;
using InvestmentManager.Service.Interfaces;
using InvestmentManager.Web.Models.ChartModels;
using InvestmentManager.Web.Models.FinancialModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    public class FinancialController : Controller
    {
        const string controllerName = "~/financial";
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;
        private readonly IInvestmentCalculator calculator;

        public FinancialController(
            IUnitOfWorkFactory unitOfWork
            , IConverterService converterService
            , IInvestmentCalculator calculator)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
            this.calculator = calculator;
        }
        public IActionResult ReportsHistory(long id) => View(id);
        public IActionResult PriceLastYear(long id) => View(id);

        public IActionResult ReportHistoryPartial(long id)
        {
            var reportModel = new List<ReportComponentModel>();
            var listReports = new List<ReportBodyModel>();

            foreach (var i in unitOfWork.Report.GetAll().Where(x => x.CompanyId == id).OrderByDescending(x => x.DateReport.Date))
            {
                listReports.Add(new ReportBodyModel
                {
                    Year = i.DateReport.Year,
                    Quarter = converterService.GetConvertedMonthInQuarter(i.DateReport.Month),
                    Assets = i.Assets,
                    CashFlow = i.CashFlow,
                    Dividends = i.Dividends,
                    GrossProfit = i.GrossProfit,
                    LongTermDebt = i.LongTermDebt,
                    NetProfit = i.NetProfit,
                    Obligations = i.Obligations,
                    Revenue = i.Revenue,
                    ShareCapital = i.ShareCapital,
                    StockVolume = i.StockVolume,
                    Turnover = i.Turnover
                });
            }

            reportModel.Add(new ReportComponentModel(listReports) { CompanyId = id });

            return PartialView(reportModel);
        }
        public IActionResult GetPrices() => View();
        public async Task<JsonResult> GetPrice(long id)
        {
            var chartModel = new BaseChartModel
            {
                Title = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Name,
                XName = "Дата",
                YName = "Цена"
            };

            var points = new Dictionary<DateTime, decimal>();

            foreach (var i in unitOfWork.Price.GetAll().Where(x => x.TickerId == id).OrderByDescending(x => x.BidDate.Date))
            {
                points.Add(i.BidDate, i.Value);
            }

            chartModel.Points = points;

            return Json(chartModel);
        }

        #region Check Reports
        [Authorize(Roles = "pestunov")]
        public IActionResult CheckReports()
        {
            var models = new List<ReportCheckModel>();
            var reports = unitOfWork.Report.GetAll().Where(x => x.IsChecked == false);
            var companies = unitOfWork.Company.GetAll();

            foreach (var i in reports.Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ReportCheckModel
            {
                Id = x.Id,
                CompanyId = x.CompanyId,
                CompanyName = y.Name,
                DateReport = x.DateReport,
                Assets = x.Assets,
                CashFlow = x.CashFlow,
                Dividends = x.Dividends,
                GrossProfit = x.GrossProfit,
                LongTermDebt = x.LongTermDebt,
                NetProfit = x.NetProfit,
                Obligations = x.Obligations,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                StockVolume = x.StockVolume,
                Turnover = x.Turnover
            }))
            {
                models.Add(i);
            }

            return View(models);
        }
        [Authorize(Roles = "pestunov")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckReports(ReportCheckModel report)
        {
            if (report is null)
                return NotFound();
            if (ModelState.IsValid)
            {
                var updatingReport = await unitOfWork.Report.FindByIdAsync(report.Id).ConfigureAwait(false);
                if (updatingReport != null)
                {
                    updatingReport.DateUpdate = DateTime.Now;
                    updatingReport.IsChecked = true;

                    updatingReport.Assets = report.Assets;
                    updatingReport.CashFlow = report.CashFlow;
                    updatingReport.Dividends = report.Dividends;
                    updatingReport.GrossProfit = report.GrossProfit;
                    updatingReport.LongTermDebt = report.LongTermDebt;
                    updatingReport.NetProfit = report.NetProfit;
                    updatingReport.Obligations = report.Obligations;
                    updatingReport.Revenue = report.Revenue;
                    updatingReport.ShareCapital = report.ShareCapital;
                    updatingReport.StockVolume = report.StockVolume;
                    updatingReport.Turnover = report.Turnover;
                }

                await unitOfWork.CompleteAsync().ConfigureAwait(false);

                return LocalRedirect($@"{controllerName}/{nameof(CheckReports)}");
            }

            return View(new List<ReportCheckModel> { report });
        }
        [Authorize(Roles = "pestunov")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCheckedReport(int id)
        {
            var deletingReport = await unitOfWork.Report.FindByIdAsync(id).ConfigureAwait(false);
            if (deletingReport != null)
            {
                unitOfWork.Report.DeleteEntity(deletingReport);
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return LocalRedirect($@"{controllerName}/{nameof(CheckReports)}");
        }
        #endregion
    }
}
