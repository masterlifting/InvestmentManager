using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        public ExchangeRatesController(IBaseRestMethod restMethod) => this.restMethod = restMethod;
        [HttpPost]
        public async Task<IActionResult> Post(ExchangeRateModel model)
        {
            var entity = new ExchangeRate
            {
                AccountId = model.AccountId,
                TransactionStatusId = model.StatusId,
                DateOperation = model.DateOperation,
                CurrencyId = model.CurrencyId,
                Identifier = model.Identifier,
                Quantity = model.Quantity,
                Rate = model.Rate
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
