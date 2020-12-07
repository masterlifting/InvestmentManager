﻿using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
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
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var prices = await unitOfWork.Price.GetCustomPricesAsync(id, 12, OrderType.OrderByDesc).ConfigureAwait(false);
            return prices is null
                ? NoContent()
                : Ok(prices.Select(x => new PriceModel
                {
                    DateUpdate = x.DateUpdate,
                    BidDate = x.BidDate,
                    Value = x.Value,
                    CurrencyId = x.CurrencyId,
                    TickerId = x.TickerId
                }).ToList());
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            var prices = await unitOfWork.Price.GetCustomPricesAsync(id, 1, OrderType.OrderBy).ConfigureAwait(false);

            if (prices is null || !prices.Any())
                return NoContent();

            var lastPrice = prices.Last();

            return Ok(new SummaryPrice
            {
                DateUpdate = lastPrice.DateUpdate,
                DatePrice = lastPrice.BidDate,
                Cost = lastPrice.Value
            });
        }
    }
}