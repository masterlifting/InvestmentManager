using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.CompanyModels;
using InvestmentManager.ViewModels.ErrorModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public CompanyController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("all")]
        public async Task<List<ViewModelBase>> GetAllCompanies() =>
            await unitOfWork.Company.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);

        [HttpGet("list")]
        public async Task<PaginationViewModelBase> GetCompanyList(int value = 1)
        {
            int pageSize = 10;
            var companies = unitOfWork.Company.GetAll().OrderBy(x => x.Name);
            var count = await companies.CountAsync().ConfigureAwait(false);
            var items = await companies.Skip((value - 1) * pageSize).Take(pageSize).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);

            var pagination = new Pagination();
            pagination.SetPagination(count, value, pageSize);
            return new PaginationViewModelBase
            {
                Items = items,
                Pagination = pagination
            };
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
        [HttpGet("transactionshort")]
        public async Task<CompanyTransactionHistoryShortModel> GetTransactionHistoryShort(long id, string values)
        {
            long[] accountIds = JsonSerializer.Deserialize<long[]>(values);
            var transaction = await unitOfWork.StockTransaction.GetAll()
                .Where(x => accountIds.Contains(x.AccountId) && x.Ticker.CompanyId == id)
                .OrderBy(x => x.DateOperation)
                .LastOrDefaultAsync();

            if (transaction != null)
                return new CompanyTransactionHistoryShortModel
                {
                    LastDateTransaction = transaction.DateOperation.ToString("g"),
                    Lot = transaction.Quantity.ToString(),
                    Price = transaction.Cost.ToString("f2"),
                    Status = transaction.TransactionStatus.Name,
                    Error = new ErrorBaseModel { IsSuccess = true }
                };
            else
                return new CompanyTransactionHistoryShortModel();
        }

        #region Company form
        [HttpGet("edit"), Authorize(Roles = "pestunov")]
        public async Task<CompanyFormModel> EditCompany(long id)
        {
            var result = new CompanyFormModel();

            var comapny = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            if (comapny is null)
                return result;

            result.Id = comapny.Id;
            result.Name = comapny.Name;
            result.DateSplit = comapny.DateSplit;
            result.IndustryId = comapny.IndustryId.ToString();
            result.SectorId = comapny.SectorId.ToString();

            result.Tickers = comapny.Tickers.Select(x => new TickerModel
            {
                Id = x.Id,
                Name = x.Name,
                ExcangeId = x.ExchangeId.ToString(),
                LotId = x.LotId.ToString()
            }).ToList();
            result.Isins = comapny.Isins.Select(x => new IsinModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            result.ReportSource = new ReportSourceModel
            {
                Id = comapny.ReportSource.Id,
                Key = comapny.ReportSource.Key,
                Value = comapny.ReportSource.Value
            };

            return result;
        }
        [HttpGet("new"), Authorize(Roles = "pestunov")]
        public async Task<CompanyFormModelData> GetNewCompany()
        {
            var result = new CompanyFormModelData
            {
                Exchanges = await unitOfWork.Exchange.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false),
                Lots = await unitOfWork.Lot.GetAll().OrderBy(x => x.Value).Select(x => new ViewModelBase { Id = x.Id, Name = x.Value.ToString() }).ToListAsync().ConfigureAwait(false),
                Industries = await unitOfWork.Industry.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false),
                Sectors = await unitOfWork.Sector.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false)
            };

            return result;
        }
        [HttpPost("save"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SaveCompany([FromBody] CompanyFormModel model)
        {
            string[] errors = new string[] { "Errors: " };
            if (ModelState.IsValid)
            {
                var company = new Company
                {
                    Name = model.Name,
                    DateSplit = model.DateSplit,
                    SectorId = long.Parse(model.SectorId),
                    IndustryId = long.Parse(model.IndustryId)
                };
                await unitOfWork.Company.CreateEntityAsync(company).ConfigureAwait(false);

                if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                {
                    foreach (var ticker in model.Tickers)
                    {
                        await unitOfWork.Ticker.CreateEntityAsync(new Ticker
                        {
                            CompanyId = company.Id,
                            Name = ticker.Name,
                            ExchangeId = long.Parse(ticker.ExcangeId),
                            LotId = long.Parse(ticker.LotId)
                        }).ConfigureAwait(false);
                    }
                    foreach (var isin in model.Isins)
                    {
                        if (!string.IsNullOrWhiteSpace(isin.Name))
                        {
                            await unitOfWork.Isin.CreateEntityAsync(new Isin
                            {
                                CompanyId = company.Id,
                                Name = isin.Name
                            }).ConfigureAwait(false);
                        }
                    }
                    await unitOfWork.ReportSource.CreateEntityAsync(new ReportSource
                    {
                        CompanyId = company.Id,
                        Key = model.ReportSource.Key,
                        Value = model.ReportSource.Value
                    }).ConfigureAwait(false);

                    if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                        return Ok(new ErrorBaseModel { IsSuccess = true });

                    return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = new string[] { "Error in company additional fields." } });
                }

                return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = errors.Append("Additional fields error.").ToArray() });
            }

            return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = errors.Append("Model validating errors.").ToArray() });
        }
        [HttpPost("update"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyFormModel model)
        {
            if (ModelState.IsValid)
            {
                string[] errors = new string[] { "Errors:" };

                var company = await unitOfWork.Company.FindByIdAsync(model.Id.Value).ConfigureAwait(false);
                if (company is null)
                    return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = new string[] { "Company not found." } });

                company.Name = model.Name;
                company.DateSplit = model.DateSplit;
                company.SectorId = long.Parse(model.SectorId);
                company.IndustryId = long.Parse(model.IndustryId);
                if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                {
                    for (int i = 0; i < model.Tickers.Count; i++)
                    {
                        var ticker = await unitOfWork.Ticker.FindByIdAsync(model.Tickers[i].Id.Value).ConfigureAwait(false);
                        if (ticker is null)
                        {
                            errors.Append($"Ticker '{model.Tickers[i].Name}' not found.");
                            continue;
                        }

                        ticker.Name = model.Tickers[i].Name;
                        ticker.ExchangeId = long.Parse(model.Tickers[i].ExcangeId);
                        ticker.LotId = long.Parse(model.Tickers[i].LotId);
                    }

                    for (int i = 0; i < model.Isins.Count; i++)
                    {
                        var isin = await unitOfWork.Isin.FindByIdAsync(model.Isins[i].Id.Value).ConfigureAwait(false);
                        if (isin is null)
                        {
                            errors.Append($"Isin '{model.Isins[i].Name}' not found.");
                            continue;
                        }
                        
                        if (!string.IsNullOrWhiteSpace(model.Isins[i].Name))
                            isin.Name = model.Isins[i].Name;
                    }

                    var reportSource = await unitOfWork.ReportSource.FindByIdAsync(model.ReportSource.Id.Value).ConfigureAwait(false);
                    if (reportSource is null)
                        errors.Append($"Reportsource '{model.ReportSource.Key}-{model.ReportSource.Value}' not found.");
                    else
                    {
                        reportSource.Key = model.ReportSource.Key;
                        reportSource.Value = model.ReportSource.Value;
                    }

                    if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                        return Ok(new ErrorBaseModel { IsSuccess = true });

                    return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = new string[] { "Additional fields error." } });
                }

                return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = errors });
            }

            return BadRequest(new ErrorBaseModel { IsSuccess = false, Errors = new string[] { "Model validating error." } });
        }
        #endregion

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
