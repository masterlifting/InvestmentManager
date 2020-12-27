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
    public class AccountTransactionsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly IReckonerService reckonerService;

        public AccountTransactionsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , IReckonerService reckonerService)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.reckonerService = reckonerService;
        }

        [HttpGet("byaccountid/{id}")]
        public async Task<IActionResult> GetByAccountId(long id)
        {
            var transactions = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.AccountTransactions;

            return transactions is null
                ? NoContent()
                : Ok(transactions.Select(x => new AccountTransactionModel
                {
                    DateOperation = x.DateOperation,
                    Amount = x.Amount,
                    StatusName = x.TransactionStatus.Name,
                    CurrencyName = x.Currency.Name,
                    AccountName = x.Account.Name.Substring(0, 9) + "..."
                }).ToList());
        }
        [HttpGet("byaccountid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountId(long id)
        {
            var transactions = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.AccountTransactions;

            if (transactions is null || !transactions.Any())
                return NoContent();

            var lastTransaction = transactions.OrderBy(x => x.DateOperation).Last();

            return Ok(new SummaryAccountTransaction
            {
                DateLastTransaction = lastTransaction.DateOperation,
                Amount = lastTransaction.Amount,
                StatusName = lastTransaction.TransactionStatus.Name,
                TotalAddedSum = transactions.Where(x => x.TransactionStatusId == 1).Sum(x => x.Amount)
            });
        }
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
            
            if (result.IsSuccess)
            {
                await reckonerService.UpgradeByAccountTransactionChangeAsync(entity).ConfigureAwait(false);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
