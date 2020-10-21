using InvestmentManager.Repository;
using InvestmentManager.ViewModels.RecommendationModels;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<RecommendationsForBuyingShortModel> GetBuyRecommendationShort(long id)
        {
            var result = await unitOfWork.BuyRecommendation.FindByIdAsync(id).ConfigureAwait(false);
            return new RecommendationsForBuyingShortModel
            {
                Price = result.Price.ToString("f2"),
                DateUpdate = result.DateUpdate.ToString("g")
            };
        }
    }
}
