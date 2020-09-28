using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels.RecommendationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IMemoryCache memoryCache;

        public RecommendationController(IUnitOfWorkFactory unitOfWork, IMemoryCache memoryCache)
        {
            this.unitOfWork = unitOfWork;
            this.memoryCache = memoryCache;
        }

        [Route("buyr")]
        [HttpGet]
        public async Task<IEnumerable<BuyRecommendationModel>> GetBuyRecommendation()
        {
            var buyRecommendations = new List<BuyRecommendationModel>();
            if (!memoryCache.TryGetValue("lastPrices", out Dictionary<long, List<Price>> lastPrices))
            {
                lastPrices = unitOfWork.Price.GetGroupedPrices(1, OrderType.OrderByDesc);
                memoryCache.Set("lastPrices", lastPrices, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
            }

            var recommendations = await unitOfWork.BuyRecommendation.GetAll().ToListAsync().ConfigureAwait(false);

            foreach (var i in recommendations.Join(lastPrices, x => x.CompanyId, y => y.Key, (x, y) => new
            {
                x.CompanyId,
                CompnyName = x.Company.Name,
                RecommendationPrice = x.Price,
                LastPriceValue = y.Value.First().Value,
                LastPriceDate = y.Value.First().DateUpdate,

                y.Value.First().CurrencyId
            }))
            {
                var recommendation = new BuyRecommendationModel
                {
                    CompanyId = i.CompanyId,
                    CompanyName = i.CompnyName,
                    LastPriceDate = i.LastPriceDate.ToString("g"),
                    BuyPrice = i.RecommendationPrice,
                    LastPriceValue = i.LastPriceValue,
                    CurrencyId = i.CurrencyId
                };

                if (i.LastPriceValue < i.RecommendationPrice)
                    recommendation.IsRecommend = true;

                buyRecommendations.Add(recommendation);
            }

            return buyRecommendations;
        }

        [Route("sellr")]
        [HttpGet]
        public async Task<IEnumerable<SellRecommendationModel>> GetSellRecommendation()
        {
            var sellRecommendations = new List<SellRecommendationModel>();

            var recommendations = await unitOfWork.SellRecommendation.GetAll().ToListAsync().ConfigureAwait(false);
            if (!memoryCache.TryGetValue("lastPrices", out Dictionary<long, List<Price>> lastPrices))
            {
                lastPrices = unitOfWork.Price.GetGroupedPrices(1, OrderType.OrderByDesc);
                memoryCache.Set("lastPrices", lastPrices, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
            }

            foreach (var i in recommendations.Join(lastPrices, x => x.CompanyId, y => y.Key, (x, y) => new
            {
                x.CompanyId,
                CompnyName = x.Company.Name,
                x.PriceMax,
                x.PriceMid,
                x.PriceMin,
                x.LotMax,
                x.LotMid,
                x.LotMin,

                LastPriceValue = y.Value.First().Value,
                y.Value.First().CurrencyId,
                LastPriceDate = y.Value.First().DateUpdate
            }))
            {
                var recommendation = new SellRecommendationModel
                {
                    CompanyId = i.CompanyId,
                    CompanyName = i.CompnyName,
                    LastPriceDate = i.LastPriceDate.ToString("g"),
                    LastPriceValue = i.LastPriceValue,
                    CurrencyId = i.CurrencyId,

                    PriceMax = i.PriceMax,
                    PriceMid = i.PriceMid,
                    PriceMin = i.PriceMin,

                    ValueMax = i.LotMax,
                    ValueMid = i.LotMid,
                    ValueMin = i.LotMin
                };

                if (i.PriceMin <= i.LastPriceValue && i.LotMin != 0)
                    recommendation.IsRecommendMin = true;
                if (i.PriceMid <= i.LastPriceValue && i.LotMid != 0)
                    recommendation.IsRecommendMid = true;
                if (i.PriceMax <= i.LastPriceValue && i.LotMax != 0)
                    recommendation.IsRecommendMax = true;

                sellRecommendations.Add(recommendation);
            }

            return sellRecommendations;
        }
    }
}
