using InvestManager.Repository;
using InvestManager.ViewModels.TransactionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace InvestManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IUnitOfWorkFactory unitOfWork;

        public TransactionController(
            UserManager<IdentityUser> userManager
            , IUnitOfWorkFactory unitOfWork)
        {
            this.userManager = userManager;
            this.unitOfWork = unitOfWork;
        }

        [Route("getstocktransactions")]
        [HttpGet]
        public async Task<IEnumerable<StockTransactionModel>> GetStockTransactions()
        {
            var accountIds = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User))).Select(x => x.Id);
            var transactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).ToListAsync().ConfigureAwait(false);

            if (!transactions.Any())
                return new List<StockTransactionModel>();

            var companies = unitOfWork.Company.GetAll();
            var tickers = unitOfWork.Ticker.GetAll();
            var lots = unitOfWork.Lot.GetAll();

            var agregateOperations = transactions
                                                   .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new
                                                   {
                                                       x.TransactionStatusId,
                                                       x.DateOperation,
                                                       x.Quantity,
                                                       x.Cost,
                                                       x.CurrencyId,
                                                       y.CompanyId,
                                                       y.LotId
                                                   })
                                                   .Join(lots, x => x.LotId, y => y.Id, (x, y) => new
                                                   {
                                                       x.TransactionStatusId,
                                                       x.DateOperation,
                                                       x.Quantity,
                                                       x.Cost,
                                                       x.CurrencyId,
                                                       x.CompanyId,
                                                       LotValue = y.Value
                                                   })
                                                   .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new
                                                   {
                                                       x.CompanyId,
                                                       x.TransactionStatusId,
                                                       x.DateOperation,
                                                       x.Quantity,
                                                       x.Cost,
                                                       x.CurrencyId,
                                                       x.LotValue,
                                                       CompanyName = y.Name
                                                   });

            return agregateOperations.GroupBy(x => new { x.CompanyId, x.CompanyName }).Select(x => new StockTransactionModel
            {
                CompanyId = x.Key.CompanyId,
                CompanyName = x.Key.CompanyName,
                FreeLot = (x.Where(y => y.TransactionStatusId == 3).Sum(x => x.Quantity / x.LotValue) - x.Where(y => y.TransactionStatusId == 4).Sum(x => x.Quantity / x.LotValue)),
                DateLastTransaction = x.OrderBy(x => x.DateOperation.Date).Last().DateOperation,
                ProfitCurrent = Math.Round(x.Where(y => y.TransactionStatusId == 4).Sum(y => y.Cost * y.Quantity) - x.Where(y => y.TransactionStatusId == 3).Sum(y => y.Cost * y.Quantity), 2),
                CurrencyId = x.First().CurrencyId
            }).OrderByDescending(x => x.DateLastTransaction);
        }
        [Route("getstocktransactiondetail")]
        [HttpGet]
        public IEnumerable<StockTransactionDetailModel> GetStockTransactionDetail(long companyId)
        {
            var accountIds = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User))).Select(x => x.Id);
            var transactions = unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId));
            var tickers = unitOfWork.Ticker.GetAll().Where(x => x.CompanyId == companyId);
            var statuses = unitOfWork.TransactionStatus.GetAll();

            return transactions
                .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new { x.Cost, x.Quantity, x.DateOperation, Ticker = y.Name, x.TransactionStatusId })
                .Join(statuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new StockTransactionDetailModel
                {
                    TickerName = x.Ticker,
                    DateTransaction = x.DateOperation,
                    Price = x.Cost,
                    Quantity = x.Quantity,
                    Status = y.Name
                }).OrderByDescending(x => x.DateTransaction);
        }

        [Route("accounttransactions")]
        [HttpGet]
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
