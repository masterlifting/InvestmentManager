using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class BuyRecommendationsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public BuyRecommendationsController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("bypagination/{value}")]
        public BaseViewPagination GetPagination(int value = 1)
        {
            int pageSize = 10;
            var companies = unitOfWork.Company.GetAll();
            var lastPricies = unitOfWork.Price.GetLastPrices(30);
            var recommendations = lastPricies
                .Join(unitOfWork.BuyRecommendation.GetAll(), x => x.Key, y => y.CompanyId, (x, y) => new
                {
                    y.CompanyId,
                    LastPrice = x.Value,
                    RecommendationPrice = y.Price,
                })
                .OrderByDescending(x => x.RecommendationPrice / x.LastPrice);

            var items = recommendations
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new BaseView { Id = y.Id, Name = y.Name })
                .ToList();

            var pagination = new Pagination();
            pagination.SetPagination(recommendations.Count(), value, pageSize);

            return new BaseViewPagination { Items = items, Pagination = pagination };
        }
        [HttpGet("bycompanyid/{id}")]
        public async Task<BuyRecommendationModel> GetByCompanyId(long id)
        {
            var result = await unitOfWork.BuyRecommendation.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id).ConfigureAwait(false);
            return result is null ? null : new BuyRecommendationModel
            {
                DateUpdate = result.DateUpdate,
                Price = result.Price
            };
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<SummaryBuyRecommendation> GetSummaryByCompanyId(long id)
        {
            var recommendation = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.BuyRecommendation;
            return recommendation is null ? new SummaryBuyRecommendation() : new SummaryBuyRecommendation
            {
                IsHave = true,
                DateUpdate = recommendation.DateUpdate,
                BuyPrice = recommendation.Price
            };
        }
    }
}
