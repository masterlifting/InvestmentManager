using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.RecommendationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;

        public RecommendationController(
            IUnitOfWorkFactory unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("buyshort")]
        public async Task<RecommendationsForBuyShortModel> GetBuyRecommendationShort(long id)
        {
            var result = await unitOfWork.BuyRecommendation.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id).ConfigureAwait(false);
            return new RecommendationsForBuyShortModel
            {
                Price = result.Price.ToString("f2"),
                DateUpdate = result.DateUpdate.ToString("g")
            };
        }
        [HttpGet("orderbuy")]
        public List<ViewModelBase> GetOrderBuyRecommendations()
        {
            var companies = unitOfWork.Company.GetAll();
            var lastPricies = unitOfWork.Price.GetLastPrices(30);
            var buyRecommendations = unitOfWork.BuyRecommendation.GetAll();

            return lastPricies
                .Join(buyRecommendations, x => x.Key, y => y.CompanyId, (x, y) => new { LastPrice = x.Value, RecommendationPrice = y.Price, y.CompanyId })
                .OrderByDescending(x => x.RecommendationPrice / x.LastPrice)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ViewModelBase { Id = y.Id, Name = y.Name }).ToList();
        }
    }
}
