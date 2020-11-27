using InvestmentManager.Entities.Broker;
using InvestmentManager.Models;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class DividendsController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        public DividendsController(IBaseRestMethod restMethod) => this.restMethod = restMethod;
        [HttpPost]
        public async Task<IActionResult> Post(DividendModel model)
        {
            var entity = new Dividend
            {
                AccountId = model.AccountId,
                Amount = model.Amount,
                Tax = model.Tax,
                IsinId = model.IsinId,
                CurrencyId = model.CurrencyId,
                DateOperation = model.DateOperation
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
