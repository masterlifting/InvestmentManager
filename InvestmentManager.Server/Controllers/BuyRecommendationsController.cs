using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class BuyRecommendationsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConfiguration configuration;

        public BuyRecommendationsController(IUnitOfWorkFactory unitOfWork, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = int.Parse(configuration["PaginationPageSize"]);
            var companies = unitOfWork.Company.GetAll();
            var lastPricies = await unitOfWork.Price.GetLastPricesAsync(30);
            var recommendations = lastPricies?
                .Join(unitOfWork.BuyRecommendation.GetAll(), x => x.Key, y => y.CompanyId, (x, y) => new
                {
                    y.CompanyId,
                    LastPrice = x.Value,
                    RecommendationPrice = y.Price,
                })
                .OrderByDescending(x => x.RecommendationPrice / x.LastPrice);

            var items = recommendations?
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView { Id = y.Id, Name = y.Name, Description = x.RecommendationPrice.ToString("#,0.####") })
                .ToList();

            if (items is null)
                return NoContent();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(recommendations.Count(), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);
        }
        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var result = await unitOfWork.BuyRecommendation.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id);
            return result is null ? NoContent() : Ok(new BuyRecommendationModel
            {
                DateUpdate = result.DateUpdate,
                Price = result.Price
            });
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            var recommendation = (await unitOfWork.Company.FindByIdAsync(id))?.BuyRecommendation;
            return recommendation is null ? NoContent() : Ok(new SummaryBuyRecommendation
            {
                DateUpdate = recommendation.DateUpdate,
                BuyPrice = recommendation.Price
            });
        }
    }
}
