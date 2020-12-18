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
    public class DividendsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly IConfiguration configuration;
        private readonly ISummaryService summaryService;
        private readonly UserManager<IdentityUser> userManager;

        public DividendsController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , IConfiguration configuration
            , ISummaryService summaryService
            , UserManager<IdentityUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
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
            var dividends = (await unitOfWork.Dividend.GetAll()
                .Where(x => accountIds.Contains(x.AccountId))
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync())
                .GroupBy(x => x.IsinId);
            var isins = unitOfWork.Isin.GetAll();
            if (dividends is null)
                return NoContent();

            var items = dividends.Skip((value - 1) * pageSize).Take(pageSize)
                .Join(isins, x => x.Key, y => y.Id, (x, y) => new { y.CompanyId, x.First().DateOperation, x.First().Amount })
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView
                {
                    Id = y.Id,
                    Name = y.Name,
                    Description = x.DateOperation.ToShortDateString() + "|" + x.Amount.ToString("f2")
                })
                .ToList();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(dividends.Count(), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);
        }

        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}")]
        public async Task<IActionResult> GetByAccountIds(long accountId, long companyId)
        {
            var transactions = await unitOfWork.Dividend.GetAll()
                .Where(x => x.AccountId == accountId && x.Isin.CompanyId == companyId)
                .ToListAsync().ConfigureAwait(false);

            return transactions is null
                ? NoContent()
                : Ok(transactions.Select(x => new DividendModel
                {
                    DateOperation = x.DateOperation,
                    Amount = x.Amount,
                    Tax = x.Tax
                }).ToList());
        }
        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountIds(long accountId, long companyId)
        {
            var dividends = await unitOfWork.Dividend.GetAll()
                .Where(x => x.AccountId == accountId && x.Isin.CompanyId == companyId)
                .OrderBy(x => x.DateOperation)
                .ToListAsync();

            if (dividends is null || !dividends.Any())
                return NoContent();

            var lastDividend = dividends.Last();

            return Ok(new SummaryDividend
            {
                DateLastDividend = lastDividend.DateOperation,
                LastAmount = lastDividend.Amount,
                TotalSum = dividends.Sum(x => x.Amount)
            });
        }
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
            if (result.IsSuccess)
            {
                await summaryService.SetDividendSummaryAsync(entity).ConfigureAwait(false);

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
