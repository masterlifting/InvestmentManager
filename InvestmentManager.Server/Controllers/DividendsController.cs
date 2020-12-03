using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public DividendsController(IUnitOfWorkFactory unitOfWork, IBaseRestMethod restMethod)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
        }

        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}")]
        public async Task<List<DividendModel>> GetByAccountIds(long accountId, long companyId)
        {
            var transactions = await unitOfWork.Dividend.GetAll()
                .Where(x => x.AccountId == accountId && x.Isin.CompanyId == companyId)
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync().ConfigureAwait(false);

            return transactions is null 
                ? new List<DividendModel>() 
                : transactions.Select(x => new DividendModel
            {
                DateOperation = x.DateOperation,
                Amount = x.Amount,
                Tax = x.Tax
            }).ToList();
        }
        [HttpGet("byaccountid/{accountId}/bycompanyid/{companyId}/summary/")]
        public async Task<SummaryDividend> GetSummaryByAccountIds(long accountId, long companyId)
        {
            var dividends = await unitOfWork.Dividend.GetAll()
                .Where(x => x.AccountId == accountId && x.Isin.CompanyId == companyId)
                .OrderBy(x => x.DateOperation)
                .ToListAsync();
            
            if (dividends is null || !dividends.Any())
                return new SummaryDividend();

            var lastDividend = dividends.Last();

            return new SummaryDividend
            {
                IsHave = true,
                DateLastDividend = lastDividend.DateOperation,
                LastAmount = lastDividend.Amount,
                TotalSum = dividends.Sum(x => x.Amount)
            };
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
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
