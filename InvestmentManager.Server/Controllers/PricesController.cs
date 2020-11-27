using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class PricesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public PricesController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("bycompanyid/{id}")]
        public async Task<List<PriceModel>> GetByCompanyId(long id)
        {
            var prices = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Tickers.FirstOrDefault()?.Prices;
            return prices is null ? null : prices.Select(x => new PriceModel
            {
                DateUpdate = x.DateUpdate,
                BidDate = x.BidDate,
                Value = x.Value,
                CurrencyId = x.CurrencyId,
                TickerId = x.TickerId
            }).ToList();
        }
        [HttpGet("bycompanyid/{id}/last/")]
        public async Task<PriceModel> GetLastByCompanyId(long id)
        {
            var price = (await unitOfWork.Price.GetCustomPricesAsync(id, 1, OrderType.OrderByDesc).ConfigureAwait(false)).FirstOrDefault();
            return price is null ? null : new PriceModel
            {
                DateUpdate = price.DateUpdate,
                BidDate = price.BidDate,
                Value = price.Value,
                CurrencyId = price.CurrencyId,
                TickerId = price.TickerId
            };
        }
    }
}