using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly ICatalogService catalogService;

        public StockTransactionsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , ICatalogService catalogService)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.catalogService = catalogService;
        }

        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}")]
        public async Task<IActionResult> GetByAccountIds(long accountId, long companyId)
        {
            var transactions = await unitOfWork.StockTransaction.GetAll()
                .Where(x => x.AccountId == accountId && x.Ticker.CompanyId == companyId)
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync().ConfigureAwait(false);

            return transactions is null || !transactions.Any()
                ? NoContent()
                : Ok(transactions.Select(x => new StockTransactionModel
                {
                    DateOperation = x.DateOperation,
                    StatusId = x.TransactionStatusId,
                    StatusName = catalogService.GetStatusName(x.TransactionStatusId),
                    Quantity = x.Quantity,
                    Cost = x.Cost
                }).ToList());
        }
        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountIds(long accountId, long companyId)
        {
            var lastTransaction = await unitOfWork.StockTransaction.GetAll()
                .Where(x => x.AccountId == accountId && x.Ticker.CompanyId == companyId)
                .OrderBy(x => x.DateOperation)
                .LastOrDefaultAsync();

            return lastTransaction is null ? NoContent() : Ok(new SummaryStockTransaction
            {
                DateTransaction = lastTransaction.DateOperation,
                StatusName = catalogService.GetStatusName(lastTransaction.TransactionStatusId),
                Quantity = lastTransaction.Quantity,
                Cost = lastTransaction.Cost
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(StockTransactionModel model)
        {
            var entity = new StockTransaction
            {
                AccountId = model.AccountId,
                Identifier = model.Identifier,
                TransactionStatusId = model.StatusId,
                Cost = model.Cost,
                Quantity = model.Quantity,
                DateOperation = model.DateOperation,
                ExchangeId = model.ExchangeId,
                TickerId = model.TickerId,
                CurrencyId = model.CurrencyId,
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
