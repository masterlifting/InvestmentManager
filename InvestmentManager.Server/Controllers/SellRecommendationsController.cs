using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class SellRecommendationsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly UserManager<IdentityUser> userManager;

        public SellRecommendationsController(
            IUnitOfWorkFactory unitOfWork
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [HttpGet("bypagination/{value}")]
        public BaseViewPagination GetPagination(int value = 1)
        {
            string userId = userManager.GetUserId(User);
            int pageSize = 10;
            var pagination = new Pagination();

            var companies = unitOfWork.Company.GetAll();
            var lastPricies = unitOfWork.Price.GetLastPrices(30);
            var recommendations = lastPricies
                .Join(unitOfWork.SellRecommendation.GetAll().Where(x => x.UserId.Equals(userId)), x => x.Key, y => y.CompanyId, (x, y) => new
                {
                    y.CompanyId,
                    LastPrice = x.Value,
                    y.PriceMin,
                    y.PriceMid,
                    y.PriceMax,
                    y.LotMin,
                    y.LotMid,
                    y.LotMax
                })
                .Where(x =>
                x.LastPrice >= x.PriceMin
                | x.LastPrice >= x.PriceMid
                | x.LastPrice >= x.PriceMax)
                .OrderBy(x => x.PriceMin / x.LastPrice)
                .ThenBy(x => x.PriceMid / x.LastPrice)
                .ThenBy(x => x.PriceMax / x.LastPrice);

            if (recommendations is null)
                return new BaseViewPagination { Items = new List<BaseView>(), Pagination = pagination };

            var items = recommendations
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new BaseView { Id = y.Id, Name = y.Name })
                .ToList();

            pagination.SetPagination(recommendations.Count(), value, pageSize);

            return new BaseViewPagination { Items = items, Pagination = pagination };
        }

        [HttpGet("bycompanyid/{id}")]
        public async Task<SellRecommendationModel> GetByCompanyId(long id)
        {
            string userId = userManager.GetUserId(User);
            var recommentation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == id).ConfigureAwait(false);

            return recommentation is null ? new SellRecommendationModel() : new SellRecommendationModel
            {
                DateUpdate = recommentation.DateUpdate,
                LotMin = recommentation.LotMin,
                LotMid = recommentation.LotMid,
                LotMax = recommentation.LotMax,
                PriceMin = recommentation.PriceMin,
                PriceMid = recommentation.PriceMid,
                PriceMax = recommentation.PriceMax
            };
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<SummarySellRecommendation> GetSummaryByCompanyId(long id)
        {
            string userId = userManager.GetUserId(User);
            var recommentation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == id).ConfigureAwait(false);

            return recommentation is null ? new SummarySellRecommendation() : new SummarySellRecommendation
            {
                IsHave = true,
                DateUpdate = recommentation.DateUpdate,
                LotMin = recommentation.LotMin,
                LotMid = recommentation.LotMid,
                LotMax = recommentation.LotMax,
                PriceMin = recommentation.PriceMin,
                PriceMid = recommentation.PriceMid,
                PriceMax = recommentation.PriceMax
            };
        }

    }
}
