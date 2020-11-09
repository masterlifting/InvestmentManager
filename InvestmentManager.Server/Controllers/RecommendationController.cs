using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ResultModels;
using InvestmentManager.ViewModels.RecommendationModels;
using Microsoft.AspNetCore.Authorization;
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
        const int pageSize = 10;

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
        [HttpGet("saleshort"), Authorize]
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
                    Error = new ResultBaseModel
                    {
                        IsSuccess = true
                    }
                };
            }
            else
                return new RecommendationsForSaleShortModel();
        }

        [HttpGet("orderbuy")]
        public PaginationViewModelBase GetOrderBuyRecommendations(int value = 1)
        {
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

            var items = recommendations.Skip((value - 1) * pageSize).Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ViewModelBase { Id = y.Id, Name = y.Name }).ToList();

            var pagination = new Pagination();
            var count = recommendations.Count();
            pagination.SetPagination(count, value, pageSize);

            return new PaginationViewModelBase
            {
                Items = items,
                Pagination = pagination
            };
        }
        [HttpGet("ordersale"), Authorize]
        public PaginationViewModelBase GetOrderSaleRecommendations(int value = 1)
        {
            var pagination = new Pagination();

            var companies = unitOfWork.Company.GetAll();
            var lastPricies = unitOfWork.Price.GetLastPrices(30);
            var recommendations = lastPricies.Join(unitOfWork.SellRecommendation.GetAll(), x => x.Key, y => y.CompanyId, (x, y) => new
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
                return new PaginationViewModelBase
                {
                    Items = new List<ViewModelBase>(),
                    Pagination = pagination
                };

            var items = recommendations.Skip((value - 1) * pageSize).Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ViewModelBase { Id = y.Id, Name = y.Name }).ToList();

            var count = recommendations.Count();
            pagination.SetPagination(count, value, pageSize);

            return new PaginationViewModelBase
            {
                Items = items,
                Pagination = pagination
            };
        }
    }
}
