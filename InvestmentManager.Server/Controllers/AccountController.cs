using InvestmentManager.ClientModels;
using InvestmentManager.ClientModels.AccountModels;
using InvestmentManager.Repository;
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
        public async Task<ClientBaseResponse<PaginationModel<ClientAccount>>> Get(int page = 1, int limit = 5, string phrase = null)
        {
            var userId = userManager.GetUserId(User);

            var query = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId));

            if (!string.IsNullOrWhiteSpace(phrase))
                query = query.Where(x => x.Name.ToLower().Contains(phrase.ToLower()));

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
