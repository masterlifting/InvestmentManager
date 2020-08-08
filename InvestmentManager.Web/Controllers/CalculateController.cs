using InvestmentManager.Repository;
using InvestmentManager.Service.Interfaces;
using InvestmentManager.Web.Models.CalculateModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Controllers
{
    public class CalculateController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;
        private readonly UserManager<IdentityUser> userManager;

        public CalculateController(
            IUnitOfWorkFactory unitOfWork
            , IConverterService converterService
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
            this.userManager = userManager;
        }

        public IActionResult Coefficients()
        {
            var model = new List<CoefficientModel>();

            foreach (var i in unitOfWork.Coefficient.GetViewData())
            {
                var tempBody = new List<CoefficientBodyModel>();
                foreach (var (dateReport, coefficient) in i.Value)
                {
                    tempBody.Add(new CoefficientBodyModel
                    {
                        Year = dateReport.Year,
                        Quarter = converterService.GetConvertedMonthInQuarter(dateReport.Month),
                        PE = coefficient.PE,
                        PB = coefficient.PB,
                        EPS = coefficient.EPS,
                        Profitability = coefficient.Profitability,
                        ROA = coefficient.ROA,
                        ROE = coefficient.ROE,
                        DebtLoad = coefficient.DebtLoad
                    });
                }

                model.Add(new CoefficientModel
                {
                    CompanyName = i.Key,
                    Coefficients = tempBody
                });
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> SellRecommendation()
        {
            var recommendations = await unitOfWork.SellRecommendation.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User))).ToListAsync().ConfigureAwait(false);

            var companies = unitOfWork.Company.GetAll();
            var lastPrices = unitOfWork.Price.GetLastPrices();

            var resultRecommendations = recommendations
                                                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new
                                                { CompanyId = y.Id, CompanyName = y.Name, Recommendation = x })
                                                .Join(lastPrices, x => x.CompanyId, y => y.Key, (x, y) => new
                                                { x.CompanyName, x.Recommendation, LastPrice = y.Value })
                                                .Select(x => new SellRecommendationModel
                                                {
                                                    CompanyName = x.CompanyName,
                                                    LastPrice = x.LastPrice,

                                                    LotMinProfit = x.Recommendation.LotMin,
                                                    LotMidProfit = x.Recommendation.LotMid,
                                                    LotMaxProfit = x.Recommendation.LotMax,

                                                    PriceMinProfit = x.Recommendation.PriceMin,
                                                    PriceMidProfit = x.Recommendation.PriceMid,
                                                    PriceMaxProfit = x.Recommendation.PriceMax
                                                });

            return View(resultRecommendations);
        }
    }
}
