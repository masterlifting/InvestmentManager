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
            var response = await webService.GetCBRateAsync().ConfigureAwait(false);
            var rate = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CBRF>().ConfigureAwait(false) : null;
            decimal dollar = rate is not null ? rate.Valute.USD.Value : 0;
            try
            {
                decimal result = await summaryService.GetAccountSumAsync(id, dollar).ConfigureAwait(false);
                return Ok(result);
            }
            catch
            {
                return BadRequest(0);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(AccountModel model)
        {
            var entity = new Account { Name = model.Name, UserId = userManager.GetUserId(User) };
            bool AccountContains(AccountModel model) => unitOfWork.Account.GetAll().Where(x => x.Name.Equals(model.Name)).Any();
            var result = await restMethod.BasePostAsync(ModelState, entity, model, AccountContains).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
