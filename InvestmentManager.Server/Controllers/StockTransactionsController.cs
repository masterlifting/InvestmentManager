using InvestmentManager.Entities.Broker;
using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration configuration;
        private readonly ISummaryService summaryService;
        private readonly UserManager<IdentityUser> userManager;

        public StockTransactionsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , ICatalogService catalogService
            , IConfiguration configuration
            , ISummaryService summaryService
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.catalogService = catalogService;
            this.configuration = configuration;
            this.summaryService = summaryService;
            this.userManager = userManager;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            string userId = userManager.GetUserId(User);
            int pageSize = int.Parse(configuration["PaginationPageSize"]);

            var companies = unitOfWork.Company.GetAll();
            var accountIds = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => x.Id);
            var transactions = (await unitOfWork.StockTransaction.GetAll()
                .Where(x => accountIds.Contains(x.AccountId))
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync())
                .GroupBy(x => x.TickerId);
            var tickers = unitOfWork.Ticker.GetAll();
            if (transactions is null)
                return NoContent();

            var items = transactions.Skip((value - 1) * pageSize).Take(pageSize)
                .Join(tickers, x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, x.First().DateOperation, x.First().TransactionStatusId })
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView
                {
                    Id = y.Id,
                    Name = y.Name,
                    Description = x.DateOperation.ToShortDateString() + $"|{catalogService.GetStatusName(x.TransactionStatusId)}"
                })
                .ToList();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(transactions.Count(), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);
        }

        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}")]
        public async Task<IActionResult> GetByAccountIds(long accountId, long companyId)
        {
            var transactions = await unitOfWork.StockTransaction.GetAll()
                .Where(x => x.AccountId == accountId && x.Ticker.CompanyId == companyId)
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

            if (result.IsSuccess)
            {
                await summaryService.SetCompanySummaryAsync(entity).ConfigureAwait(false);
                
                await summaryService.SetAccountFreeSumAsync(entity.AccountId, entity.CurrencyId).ConfigureAwait(false);

                bool isComplete = await unitOfWork.CompleteAsync().ConfigureAwait(false);
                if (!isComplete)
                    result.Info += "; SUMMARY ERROR!";

                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
