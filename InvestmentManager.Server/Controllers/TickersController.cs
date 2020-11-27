using InvestmentManager.Entities.Market;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class TickersController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;

        public TickersController(IUnitOfWorkFactory unitOfWork, IBaseRestMethod restMethod)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
        }
        
        [HttpGet]
        public async Task<List<TickerModel>> Get() =>
            await unitOfWork.Ticker.GetAll().Select(x => new TickerModel
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId = x.CompanyId,
                ExchangeId = x.ExchangeId,
                LotId = x.LotId,
            }).ToListAsync().ConfigureAwait(false);
        [HttpGet("bycompanyid/{id}")]
        public async Task<List<TickerModel>> GetByCompanyId(long id)
        {
            var tickers = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Tickers;
            return tickers is null ? null : tickers.Select(x => new TickerModel
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId = x.CompanyId,
                ExchangeId = x.ExchangeId,
                LotId = x.LotId,
                IsEditeble = true
            }).ToList();
        }
        [HttpPost, Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Post(TickerModel model)
        {
            var entity = new Ticker { Name = model.Name, CompanyId = model.CompanyId, ExchangeId = model.ExchangeId, LotId = model.LotId };
            bool TickerContains(TickerModel model) => unitOfWork.Ticker.GetAll().Where(x => x.Name.Equals(model.Name)).Any();
            var result = await restMethod.BasePostAsync(ModelState, entity, model, TickerContains).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, TickerModel model)
        {
            void UpdateTicker(Ticker ticker)
            {
                ticker.DateUpdate = DateTime.Now;
                ticker.Name = model.Name;
                ticker.ExchangeId = model.ExchangeId;
                ticker.LotId = model.LotId;
            }

            var result = await restMethod.BasePutAsync<Ticker>(ModelState, id, UpdateTicker);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
        [HttpDelete("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await restMethod.BaseDeleteAsync<Ticker>(id).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
