using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ErrorModels;
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
        [HttpGet("saleshort")]
        public async Task<RecommendationsForSaleShortModel> GetSellRecommendationShort(long id)
        {
            var result = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id).ConfigureAwait(false);
            if (result != null)
            {
                return new RecommendationsForSaleShortModel
                {
                    DateUpdate = result.DateUpdate.ToString("g"),

                    LotMin = result.LotMin,
                    LotMid = result.LotMid,
                    LotMax = result.LotMax,

                    PriceMin = result.PriceMin.ToString("f2"),
                    PriceMid = result.PriceMid.ToString("f2"),
                    PriceMax = result.PriceMax.ToString("f2"),
                    Error = new ErrorBaseModel
                    {
                        IsSuccess = true
                    }
                };
            }
            else
                return new RecommendationsForSaleShortModel();
        }

        [HttpGet("orderbuy")]
        public async Task<PaginationViewModelBase> GetOrderBuyRecommendations(int value = 1)
        {
            int pageSize = 10;
            var companies = unitOfWork.Company.GetAll();
            var lastPricies = unitOfWork.Price.GetLastPrices(30);
            var buyRecommendations = unitOfWork.BuyRecommendation.GetAll();

            var pagination = new Pagination();
            var count = await buyRecommendations.CountAsync().ConfigureAwait(false);
            pagination.SetPagination(count, value, pageSize);

            var items = lastPricies
                .Join(buyRecommendations, x => x.Key, y => y.CompanyId, (x, y) => new { LastPrice = x.Value, RecommendationPrice = y.Price, y.CompanyId })
                .OrderByDescending(x => x.RecommendationPrice / x.LastPrice)
                .Skip((value - 1) * pageSize).Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ViewModelBase { Id = y.Id, Name = y.Name }).ToList();

            return new PaginationViewModelBase
            {
                Items = items,
                Pagination = pagination
            };
        }
    }
}
