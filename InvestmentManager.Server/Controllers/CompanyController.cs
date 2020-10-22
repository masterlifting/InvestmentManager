using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private static int _size = 0;
        private readonly IUnitOfWorkFactory unitOfWork;
        public CompanyController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("list")]
        public async Task<List<ViewModelBase>> GetCompanyList()
        {
            return await unitOfWork.Company.GetAll()
                .OrderBy(x => x.Name)
                .Select(x => new ViewModelBase { Id = x.Id, Name = x.Name })
                .ToListAsync().ConfigureAwait(false);
        }
        [HttpGet("listsize")]
        public async Task<List<ViewModelBase>> GetCompanyList(int size)
        {
            var result = await unitOfWork.Company.GetAll()
                .OrderBy(x => x.Name)
                .Skip(_size)
                .Take(size)
                .Select(x => new ViewModelBase { Id = x.Id, Name = x.Name })
                .ToListAsync().ConfigureAwait(false);

            _size += size;

            return result;
        }
        [HttpGet("additionalshort")]
        public async Task<CompanyAdditionalInfoShortModel> GetAdditionalShort(long id)
        {
            var result = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            return new CompanyAdditionalInfoShortModel
            {
                Industry = result.Industry.Name,
                Sector = result.Sector.Name,
                Currency = result.Tickers.FirstOrDefault().Prices.FirstOrDefault().Currency.Name
            };
        }

        #region Old
        //[HttpGet("getstocktransactions")]
        //public async Task<IEnumerable<StockTransactionModel>> GetStockTransactions()
        //{
        //    var accountIds = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User))).Select(x => x.Id);
        //    var transactions = await unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId)).ToListAsync().ConfigureAwait(false);

        //    if (!transactions.Any())
        //        return new List<StockTransactionModel>();

        //    var companies = unitOfWork.Company.GetAll();
        //    var tickers = unitOfWork.Ticker.GetAll();
        //    var lots = unitOfWork.Lot.GetAll();

        //    var agregateOperations = transactions
        //                                           .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new
        //                                           {
        //                                               x.TransactionStatusId,
        //                                               x.DateOperation,
        //                                               x.Quantity,
        //                                               x.Cost,
        //                                               x.CurrencyId,
        //                                               y.CompanyId,
        //                                               y.LotId
        //                                           })
        //                                           .Join(lots, x => x.LotId, y => y.Id, (x, y) => new
        //                                           {
        //                                               x.TransactionStatusId,
        //                                               x.DateOperation,
        //                                               x.Quantity,
        //                                               x.Cost,
        //                                               x.CurrencyId,
        //                                               x.CompanyId,
        //                                               LotValue = y.Value
        //                                           })
        //                                           .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new
        //                                           {
        //                                               x.CompanyId,
        //                                               x.TransactionStatusId,
        //                                               x.DateOperation,
        //                                               x.Quantity,
        //                                               x.Cost,
        //                                               x.CurrencyId,
        //                                               x.LotValue,
        //                                               CompanyName = y.Name
        //                                           });

        //    return agregateOperations.GroupBy(x => new { x.CompanyId, x.CompanyName }).Select(x => new StockTransactionModel
        //    {
        //        CompanyId = x.Key.CompanyId,
        //        CompanyName = x.Key.CompanyName,
        //        FreeLot = (x.Where(y => y.TransactionStatusId == 3).Sum(x => x.Quantity / x.LotValue) - x.Where(y => y.TransactionStatusId == 4).Sum(x => x.Quantity / x.LotValue)),
        //        DateLastTransaction = x.OrderBy(x => x.DateOperation.Date).Last().DateOperation,
        //        ProfitCurrent = Math.Round(x.Where(y => y.TransactionStatusId == 4).Sum(y => y.Cost * y.Quantity) - x.Where(y => y.TransactionStatusId == 3).Sum(y => y.Cost * y.Quantity), 2),
        //        CurrencyId = x.First().CurrencyId
        //    }).OrderByDescending(x => x.DateLastTransaction);
        //}
        //[HttpGet("getstocktransactiondetail")]
        //public IEnumerable<StockTransactionDetailModel> GetStockTransactionDetail(long companyId)
        //{
        //    var accountIds = unitOfWork.Account.GetAll().Where(x => x.UserId.Equals(userManager.GetUserId(User))).Select(x => x.Id);
        //    var transactions = unitOfWork.StockTransaction.GetAll().Where(x => accountIds.Contains(x.AccountId));
        //    var tickers = unitOfWork.Ticker.GetAll().Where(x => x.CompanyId == companyId);
        //    var statuses = unitOfWork.TransactionStatus.GetAll();

        //    return transactions
        //        .Join(tickers, x => x.TickerId, y => y.Id, (x, y) => new { x.Cost, x.Quantity, x.DateOperation, Ticker = y.Name, x.TransactionStatusId })
        //        .Join(statuses, x => x.TransactionStatusId, y => y.Id, (x, y) => new StockTransactionDetailModel
        //        {
        //            TickerName = x.Ticker,
        //            DateTransaction = x.DateOperation,
        //            Price = x.Cost,
        //            Quantity = x.Quantity,
        //            Status = y.Name
        //        }).OrderByDescending(x => x.DateTransaction);
        //}
        #endregion
    }
}
