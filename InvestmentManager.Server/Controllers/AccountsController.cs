using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ISummaryService summaryService;

        public AccountsController(
            UserManager<IdentityUser> userManager
            , IBaseRestMethod restMethod
            , IUnitOfWorkFactory unitOfWork
            , ISummaryService summaryService)
        {
            this.userManager = userManager;
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
            this.summaryService = summaryService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = userManager.GetUserId(User);
            var accounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId));
            return accounts is null
                ? NoContent()
                : Ok(await accounts.Select(x => new AccountModel { Id = x.Id, Name = x.Name }).ToListAsync());
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            Account account = await unitOfWork.Account.FindByIdAsync(id);
            return account is null
                ? NoContent()
                : Ok(new AccountModel { Id = account.Id, Name = account.Name });
        }
        [HttpGet("{id}/summary/")]
        public async Task<IActionResult> GetSum(long id) => Ok(await summaryService.GetAccountSumAsync(id));
        [HttpGet("{id}/additional/")]
        public async Task<IActionResult> GetAdditional(long id)
        {
            var account = await unitOfWork.Account.FindByIdAsync(id);
            var summaries = account?.AccountSummaries;

            if (summaries is null || !summaries.Any())
                return NoContent();

            var dividendSummaries = account.DividendSummaries;

            return Ok(new AccountAdditionalModel
            {
                Details = summaries.Select(x => new AccountAdditionalDetail
                {
                    Currency = x.Currency.Name,
                    FreeSum = x.FreeSum,
                    InvestedSum = x.InvestedSum,
                    DividendSum = dividendSummaries.Where(y => y.CurrencyId == x.CurrencyId).Any() ? dividendSummaries.Where(y => y.CurrencyId == x.CurrencyId).Sum(x => x.TotalSum) : 0
                }).ToList()
            });
        }


        [HttpPost]
        public async Task<IActionResult> Post(AccountModel model)
        {
            var entity = new Account { Name = model.Name, UserId = userManager.GetUserId(User) };
            async Task<bool> AccountValidatorAsync(AccountModel model) =>
                !await unitOfWork.Account.GetAll().Where(x => x.Name.Equals(model.Name)).AnyAsync();
            var result = await restMethod.BasePostAsync(ModelState, entity, model, AccountValidatorAsync);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
