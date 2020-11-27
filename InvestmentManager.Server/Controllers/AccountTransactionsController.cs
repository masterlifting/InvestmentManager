using InvestmentManager.Entities.Broker;
using InvestmentManager.Models;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class AccountTransactionsController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        public AccountTransactionsController(IBaseRestMethod restMethod) => this.restMethod = restMethod;
        [HttpPost]
        public async Task<IActionResult> Post(AccountTransactionModel model)
        {
            var entity = new AccountTransaction
            {
                AccountId = model.AccountId,
                Amount = model.Amount,
                CurrencyId = model.CurrencyId,
                DateOperation = model.DateOperation,
                TransactionStatusId = model.StatusId,
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
