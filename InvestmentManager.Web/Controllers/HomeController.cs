using InvestmentManager.Repository;
using InvestmentManager.Web.Models.CommonModels;
using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IFinancialAgregator financialAgregator;

        public HomeController(
            IUnitOfWorkFactory unitOfWork
            , IFinancialAgregator financialAgregator)
        {
            this.unitOfWork = unitOfWork;
            this.financialAgregator = financialAgregator;
        }
        public async Task<IActionResult> Index()
        {
            var startModel = new List<StartPageComponentModel>();

            var companies = await unitOfWork.Company.GetAll().ToListAsync().ConfigureAwait(false);
            var sectors = unitOfWork.Sector.GetAll();
            var industries = unitOfWork.Industry.GetAll();
            var reportSource = unitOfWork.ReportSource.GetAll();
            var lastPrices = unitOfWork.Price.GetLastPrices();
            var ratings = unitOfWork.Rating.GetAll();
            var buyRecommendation = unitOfWork.BuyRecommendation.GetAll();
            var financial = await financialAgregator.GetReportsComponentAsync(0).ConfigureAwait(false);

            foreach
            (
                var i in companies
                .Join(sectors, x => x.SectorId, y => y.Id, (x, y) => new { CompanyId = x.Id, SectorId = y.Id, CompanyName = x.Name, x.IndustryId, Sector = y.Name })
                .Join(industries, x => x.IndustryId, y => y.Id, (x, y) => new { IndustryId = y.Id, x.CompanyId, x.SectorId, x.CompanyName, x.Sector, Industry = y.Name })
                .Join(reportSource, x => x.CompanyId, y => y.CompanyId, (x, y) => new { ReportSource = y.Value, x.CompanyName, x.Sector, x.Industry, x.CompanyId, x.IndustryId, x.SectorId })
                .Join(lastPrices, x => x.CompanyId, y => y.Key, (x, y) => new { LastPrice = y.Value, x.ReportSource, x.CompanyName, x.Sector, x.Industry, x.CompanyId, x.IndustryId, x.SectorId })
                .Join(financial, x => x.CompanyId, y => y.CompanyId, (x, y) => new { x.LastPrice, x.ReportSource, x.CompanyName, x.Sector, x.Industry, x.CompanyId, x.IndustryId, x.SectorId, y.LastYear, y.LastQuarter })
                .Join(ratings, x => x.CompanyId, y => y.CompanyId, (x, y) => new { x.LastPrice, x.ReportSource, x.CompanyName, x.Sector, x.Industry, x.CompanyId, x.IndustryId, x.SectorId, x.LastYear, x.LastQuarter, y.Place })
                .Join(buyRecommendation, x => x.CompanyId, y => y.CompanyId, (x, y) => new { x.LastPrice, x.ReportSource, x.CompanyName, x.Sector, x.Industry, x.CompanyId, x.IndustryId, x.SectorId, x.LastYear, x.LastQuarter, x.Place, BuyPrice = y.Price })
            )
            {
                startModel.Add(new StartPageComponentModel
                {
                    CompanyId = i.CompanyId,
                    SectorId = i.SectorId,
                    IndustryId = i.IndustryId,

                    LastPrice = i.LastPrice,
                    BuyPrice = i.BuyPrice,

                    LastYearReport = i.LastYear,
                    LastQuarterReport = i.LastQuarter,

                    CompanyName = i.CompanyName,
                    IndustryName = i.Industry,
                    SectorName = i.Sector,
                    ReportSource = i.ReportSource,
                    Place = i.Place
                });
            }

            return View(startModel.OrderBy(x => x.Place));
        }

        public IActionResult Work()
        {
            return RedirectToAction("Index");
        }
    }
}

