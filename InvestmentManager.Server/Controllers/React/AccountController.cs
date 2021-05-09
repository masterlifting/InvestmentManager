using InvestmentManager.ClientModels;
using InvestmentManager.ClientModels.AccountModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.JwtService;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), JwtAuthorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ISummaryService summaryService;

        public AccountController(UserManager<IdentityUser> userManager, IUnitOfWorkFactory unitOfWork, ISummaryService summaryService)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.summaryService = summaryService;
        }
        public async Task<ClientBaseResponse<PaginationModel<ClientAccount>>> Get(int page = 1, int limit = 5)
        {
            var userId = userManager.GetUserId(User);

            var query = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId));

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(x => new ClientAccount { Id = x.Id, Name = x.Name })
                .ToArrayAsync();

            for (int i = 0; i < items.Length; i++)
                items[i].Sum = await summaryService.GetAccountSumAsync(items[i].Id);

            return new()
            {
                IsSuccess = true,
                Data = new()
                {
                    Items = items,
                    TotalCount = totalCount
                }
            };
        }
    }
}
