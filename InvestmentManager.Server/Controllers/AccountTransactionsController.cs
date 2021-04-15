using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

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
            var transactions = (await unitOfWork.Account.FindByIdAsync(id))?.AccountTransactions;

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
            var transactions = (await unitOfWork.Account.FindByIdAsync(id))?.AccountTransactions;

            if (transactions is null || !transactions.Any())
                return NoContent();

            return Ok(new SummaryAccountTransaction
            {
                Details = transactions.GroupBy(x => x.Currency.Name).Select(x => new SummaryAccountTransactionDetail
                {
                    Currency = x.Key,
                    AddedSum = x.Where(z => z.TransactionStatusId == (long)TransactionStatusTypes.Add).Sum(z => z.Amount),
                    WithdrawnSum = x.Where(z => z.TransactionStatusId == (long)TransactionStatusTypes.Withdraw).Sum(z => z.Amount)
                }).ToList()
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

            async Task<bool> TransactionValidatorAsync(AccountTransactionModel model)
            {
                if (model.StatusId == (long)TransactionStatusTypes.Withdraw)
                {
                    var summary = await unitOfWork.AccountSummary.GetAll()
                        .FirstOrDefaultAsync(x =>
                        x.AccountId == model.AccountId
                        && x.CurrencyId == model.CurrencyId)
                        ;

                    return summary is not null && model.Amount <= summary.FreeSum;
                }
                else
                    return true;
            }
            var result = await restMethod.BasePostAsync(ModelState, entity, model, TransactionValidatorAsync);

            if (result.IsSuccess)
            {
                result.Info += await reckonerService.UpgradeByAccountTransactionChangeAsync(entity) ? " Recalculated" : " NOT Recalculated.";

                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
