using InvestmentManager.Entities.Broker;
using InvestmentManager.Models;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ComissionsController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        public ComissionsController(IBaseRestMethod restMethod) => this.restMethod = restMethod;
        [HttpPost]
        public async Task<IActionResult> Post(ComissionModel model)
        {
            var entity = new Comission
            {
                AccountId = model.AccountId,
                CurrencyId = model.CurrencyId,
                ComissionTypeId = model.TypeId,
                Amount = model.Amount,
                DateOperation = model.DateOperation
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
