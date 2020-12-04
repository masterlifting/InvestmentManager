using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class AccountTransactionsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly ICatalogService catalogService;

        public AccountTransactionsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , ICatalogService catalogService)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.catalogService = catalogService;
        }

        [HttpGet("byaccountid/{id}")]
        public async Task<List<AccountTransactionModel>> GetByAccountId(long id)
        {
            var transactions = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.AccountTransactions;

            return transactions is null
                ? new List<AccountTransactionModel>()
                : transactions.Select(x => new AccountTransactionModel
                {
                    DateOperation = x.DateOperation,
                    Amount = x.Amount,
                    StatusName = catalogService.GetStatusName(x.TransactionStatusId),
                    CurrencyName = catalogService.GetCurrencyName(x.CurrencyId),
                    AccountName = x.Account.Name.Substring(0, 9) + "..."
                }).ToList();
        }
        [HttpGet("byaccountid/{id}/summary/")]
        public async Task<SummaryAccountTransaction> GetSummaryByAccountId(long id)
        {
            var transactions = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.AccountTransactions;

            if (transactions is null || !transactions.Any())
                return new SummaryAccountTransaction();

            var lastTransaction = transactions.OrderBy(x => x.DateOperation).Last();

            return new SummaryAccountTransaction
            {
                IsHave = true,
                DateLastTransaction = lastTransaction.DateOperation,
                Amount = lastTransaction.Amount,
                StatusName = catalogService.GetStatusName(lastTransaction.TransactionStatusId),
                TotalAddedSum = transactions.Where(x => x.TransactionStatusId == 1).Sum(x => x.Amount)
            };
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
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
