using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels.AccountModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"),Authorize]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ISummaryService summaryService;

        public AccountController(
            UserManager<IdentityUser> userManager
            , IUnitOfWorkFactory unitOfWork
            , ISummaryService summaryService)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
            this.summaryService = summaryService;
        }

        [HttpGet("all")]
        public async Task<List<AccountFrameModel>> GetAllAccontInfo()
        {
            string userId = userManager.GetUserId(User);
            var models = await unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userId)).Select(x => new AccountFrameModel
            {
                Id = x.Id,
                Name = x.Name,
                Sum = 0,
                IsActive = true
            }).ToListAsync().ConfigureAwait(false);

            for (int i = 0; i < models.Count; i++)
                models[i].Sum = await summaryService.GetAccountSumAsync(models[i].Id).ConfigureAwait(false);

            return models;
        }
        [HttpGet("selected")]
        public async Task<List<AccountFrameModel>> GetSelectedAccountInfo(string ids)
        {
            var Ids = JsonSerializer.Deserialize<long[]>(ids);
            var models = await unitOfWork.Account.GetAll().Where(x => Ids.Contains(x.Id)).Select(x => new AccountFrameModel
            {
                Id = x.Id,
                Name = x.Name,
                Sum = 0,
                IsActive = true
            }).ToListAsync().ConfigureAwait(false);

            for (int i = 0; i < models.Count; i++)
                models[i].Sum = await summaryService.GetAccountSumAsync(models[i].Id).ConfigureAwait(false);

            return models;
        }
        [HttpGet("transactions")]
        public IEnumerable<AccountTransactionModel> GetAccountTransactions()
        {
            var accounts = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User)));
            var transactions = unitOfWork.AccountTransaction.GetAll().Where(x => accounts.Select(y => y.Id).Contains(x.AccountId));
            var statuses = unitOfWork.TransactionStatus.GetAll();

            return transactions
                    .Join(statuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new { x.AccountId, x.CurrencyId, x.Amount, x.DateOperation, TypeOperation = y.Name })
                    .Join(accounts, x => x.AccountId, y => y.Id, (x, y) => new AccountTransactionModel
                    {
                        Account = y.Name,
                        Sum = x.Amount,
                        DateTransaction = x.DateOperation,
                        CurrencyId = x.CurrencyId,
                        Status = x.TypeOperation
                    })
                    .OrderByDescending(x => x.DateTransaction);
        }
    }
}
