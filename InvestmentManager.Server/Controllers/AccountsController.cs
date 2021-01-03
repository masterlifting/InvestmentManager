using InvestmentManager.Entities.Broker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Repository;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.Models.Additional;
using System.Net.Http.Json;
using InvestmentManager.Models.EntityModels;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ISummaryService summaryService;
        private readonly IWebService webService;

        public AccountsController(
            UserManager<IdentityUser> userManager
            , IBaseRestMethod restMethod
            , IUnitOfWorkFactory unitOfWork
            , ISummaryService summaryService
            , IWebService webService)
        {
            this.userManager = userManager;
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
            this.summaryService = summaryService;
            this.webService = webService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = userManager.GetUserId(User);
            var accounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId));
            return accounts is null
                ? NoContent()
                : Ok(await accounts.Select(x => new AccountModel { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false));
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            Account account = await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false);
            return account is null
                ? NoContent()
                : Ok(new AccountModel { Id = account.Id, Name = account.Name });
        }
        [HttpGet("{id}/summary/")]
        public async Task<IActionResult> GetSum(long id)
        {
            decimal result = 0;

            try
            {
                long[] currencyIds = await unitOfWork.Currency.GetAll().Select(x => x.Id).ToArrayAsync().ConfigureAwait(false);

                foreach (var currencyId in currencyIds)
                {
                    decimal intermediateResult = await summaryService.GetAccountTotalSumAsync(id, currencyId).ConfigureAwait(false);
                    decimal rateValue = 1;

                    if (currencyId != (long)CurrencyTypes.rub)
                    {
                        var response = await webService.GetCBRateAsync().ConfigureAwait(false);
                        var rate = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CBRF>().ConfigureAwait(false) : null;

                        rateValue = rate is not null
                            ? currencyId switch
                            {
                                (long)CurrencyTypes.usd => rate.Valute.USD.Value,
                                //(long)CurrencyTypes.RUB => rate.Valute.EUR.Value,
                                _ => 0
                            }
                            : 0;
                    }

                    result += intermediateResult * rateValue;
                }

                return Ok(result);
            }
            catch
            {
                return BadRequest(result);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(AccountModel model)
        {
            var entity = new Account { Name = model.Name, UserId = userManager.GetUserId(User) };
            async Task<bool> AccountValidatorAsync(AccountModel model) =>
                !await unitOfWork.Account.GetAll().Where(x => x.Name.Equals(model.Name)).AnyAsync().ConfigureAwait(false);
            var result = await restMethod.BasePostAsync(ModelState, entity, model, AccountValidatorAsync).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
