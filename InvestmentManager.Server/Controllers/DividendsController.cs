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
    public class DividendsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        public DividendsController(IUnitOfWorkFactory unitOfWork, IBaseRestMethod restMethod)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
        }

        [HttpGet("byaccountids/{values}/bycompanyid/{id}/last/")]
        public async Task<DividendModel> GetLastByAccountIds(string values, long id)
        {
            long[] accountIds = JsonSerializer.Deserialize<long[]>(values);
            var lastDividend = await unitOfWork.Dividend.GetAll()
                .Where(x => accountIds.Contains(x.AccountId) && x.Isin.CompanyId == id)
                .OrderBy(x => x.DateOperation)
                .LastOrDefaultAsync();

            return lastDividend is not null
                ? new DividendModel
                {
                    IsHave = true,
                    DateOperation = lastDividend.DateOperation,
                    Amount = lastDividend.Amount,
                    Tax = lastDividend.Tax
                }
                : new DividendModel { IsHave = false };
        }
        [HttpGet("byaccountids/{values}/bycompanyid/{id}")]
        public async Task<List<DividendModel>> GetByAccountIds(string values, long id)
        {
            long[] accountIds = JsonSerializer.Deserialize<long[]>(values);
            var transactions = await unitOfWork.Dividend.GetAll()
                .Where(x => accountIds.Contains(x.AccountId) && x.Isin.CompanyId == id)
                .OrderByDescending(x => x.DateOperation)
                .ToListAsync().ConfigureAwait(false);

            return transactions is null ? null
                : transactions.Select(x => new DividendModel
                {
                    IsHave = true,
                    DateOperation = x.DateOperation,
                    Amount = x.Amount,
                    Tax = x.Tax
                }).ToList();
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
