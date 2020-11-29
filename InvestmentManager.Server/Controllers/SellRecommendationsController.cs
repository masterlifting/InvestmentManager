using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("pagination/{value}")]
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

            if (!unitOfWork.SellRecommendation.GetAll().Where(x => x.UserId.Equals(userId)).Any())
                return new SellRecommendationModel { IsHave = false };

            var entity = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.SellRecommendation;
            return entity is null
                ? new SellRecommendationModel { IsHave = false }
                : new SellRecommendationModel
                {
                    IsHave = true,
                    DateUpdate = entity.DateUpdate,
                    LotMin = entity.LotMin,
                    LotMid = entity.LotMid,
                    LotMax = entity.LotMax,
                    PriceMin = entity.PriceMin,
                    PriceMid = entity.PriceMid,
                    PriceMax = entity.PriceMax
                };
        }
    }
}
