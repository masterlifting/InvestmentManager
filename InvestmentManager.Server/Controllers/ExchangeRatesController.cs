using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IReckonerService reckonerService;

        public ExchangeRatesController(
            IBaseRestMethod restMethod
            , IUnitOfWorkFactory unitOfWork
            , IReckonerService reckonerService)
        {
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
            this.reckonerService = reckonerService;
        }

        [HttpGet("byaccountid/{id}")]
        public async Task<IActionResult> GetByAccountId(long id)
        {
            var rates = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.ExchangeRates;

            return rates is null
                ? NoContent()
                : Ok(rates.Select(x => new ExchangeRateModel
                {
                    DateOperation = x.DateOperation,
                    StatusName = x.TransactionStatus.Name,
                    Rate = x.Rate,
                    Quantity = x.Quantity
                }).ToList());
        }
        [HttpGet("byaccountid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountId(long id)
        {
            var rates = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.ExchangeRates;

            if (rates is null || !rates.Any())
                return NoContent();

            var lastRate = rates.OrderBy(x => x.DateOperation).Last();

            return Ok(new SummaryExchangeRate
            {
                DateLastOperation = lastRate.DateOperation,
                Rate = lastRate.Rate,
                StatusName = lastRate.TransactionStatus.Name
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(ExchangeRateModel model)
        {
            var entity = new ExchangeRate
            {
                AccountId = model.AccountId,
                CurrencyId = model.CurrencyId,
                TransactionStatusId = model.StatusId,
                DateOperation = model.DateOperation,
                Identifier = model.Identifier,
                Quantity = model.Quantity,
                Rate = model.Rate
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await reckonerService.UpgradeByExchangeRateChangeAsync(entity).ConfigureAwait(false);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
