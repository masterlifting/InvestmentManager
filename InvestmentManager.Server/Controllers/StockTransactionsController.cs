using InvestmentManager.Entities.Broker;
using InvestmentManager.Models;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class StockTransactionsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        public StockTransactionsController(IUnitOfWorkFactory unitOfWork, IBaseRestMethod restMethod)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
        }

        [HttpGet("byaccountids/{values}/bycompanyid/{id}/last/")]
        public async Task<StockTransactionModel> GetLastByAccountIds(string values, long id)
        {
            long[] accountIds = JsonSerializer.Deserialize<long[]>(values);
            var lastTransaction = await unitOfWork.StockTransaction.GetAll()
                .Where(x => accountIds.Contains(x.AccountId) && x.Ticker.CompanyId == id)
                .OrderBy(x => x.DateOperation)
                .LastOrDefaultAsync();

            return lastTransaction is not null
                ? new StockTransactionModel
                {
                    IsHave = true,
                    DateOperation = lastTransaction.DateOperation,
                    StatusId = lastTransaction.TransactionStatusId,
                    StatusName = lastTransaction.TransactionStatusId == 3 ? "Buy" : "Sell",
                    Quantity = lastTransaction.Quantity,
                    Cost = lastTransaction.Cost
                }
                : new StockTransactionModel { IsHave = false };
        }
        [HttpGet("byaccountids/{values}/bycompanyid/{id}")]
        public async Task<List<StockTransactionModel>> GetByAccountIds(string values, long id)
        {
            long[] accountIds = JsonSerializer.Deserialize<long[]>(values);
            var transactions = await unitOfWork.StockTransaction.GetAll()
                .Where(x => accountIds.Contains(x.AccountId) && x.Ticker.CompanyId == id)
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync().ConfigureAwait(false);

            return transactions is null ? null
                : transactions.Select(x => new StockTransactionModel
                {
                    IsHave = true,
                    DateOperation = x.DateOperation,
                    StatusId = x.TransactionStatusId,
                    StatusName = x.TransactionStatusId == 3 ? "Buy" : "Sell",
                    Quantity = x.Quantity,
                    Cost = x.Cost
                }).ToList();
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
