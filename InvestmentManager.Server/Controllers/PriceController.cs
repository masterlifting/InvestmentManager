using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InvestmentManager.Entities.Market;
using InvestmentManager.PriceFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels.ResultModels;
using InvestmentManager.ViewModels.PriceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class PriceController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IPriceService priceService;

        public PriceController(
            IUnitOfWorkFactory unitOfWork
            , IPriceService priceService)
        {
            this.unitOfWork = unitOfWork;
            this.priceService = priceService;
        }

        [HttpGet("new"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> GetNewPrices()
        {
            var newPricies = new List<Price>();
            var exchanges = unitOfWork.Exchange.GetAll();
            var tickers = unitOfWork.Ticker.GetPriceTikers();
            var priceConfigure = tickers.Join(exchanges, x => x.ExchangeId, y => y.Id, (x, y) => new { TickerId = x.Id, Ticker = x.Name, y.ProviderName, y.ProviderUri });

            int count = priceConfigure.Count();

            foreach (var i in priceConfigure)
            {
                try
                {
                    var newPrice = await priceService.GetPriceListAsync(i.ProviderName, i.TickerId, i.Ticker, i.ProviderUri).ConfigureAwait(false);
                    newPricies.AddRange(newPrice);
                }
                catch
                {
                    continue;
                }
            }

            await unitOfWork.Price.CreateEntitiesAsync(newPricies).ConfigureAwait(false);
            try
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }
            catch
            {
                unitOfWork.Price.PostgresAutoReseed();
            }
            finally
            {
                await unitOfWork.CompleteAsync().ConfigureAwait(false);
            }

            return Ok();
        }

        [HttpGet("short")]
        public async Task<PriceShortModel> GetHistoryShort(long id)
        {
            var price = (await unitOfWork.Price.GetCustomPricesAsync(id, 1, OrderType.OrderByDesc).ConfigureAwait(false)).FirstOrDefault();
            if (price != null)
                return new PriceShortModel { DateUpdate = price.BidDate.ToString("g"), LastPrice = price.Value.ToString("f2"), Error = new ResultBaseModel { IsSuccess = true } };
            else
                return new PriceShortModel { Error = new ResultBaseModel { Errors = new string[] { "Maybe the price is out of date." } } };
        }
        [HttpGet("full")]
        public async Task<List<PriceFullModel>> GetFull(long id)
        {
            var prices = await unitOfWork.Price.GetCustomPricesAsync(id, 12, OrderType.OrderByDesc).ConfigureAwait(false);
            return prices.Select(x => new PriceFullModel
            {
                DateBid = x.BidDate,
                Price = x.Value
            }).ToList();
        }
    }
}
