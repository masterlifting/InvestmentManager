using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.CompanyModels;
using InvestmentManager.ViewModels.ResultModels;
using InvestmentManager.ViewModels.FormEntityModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

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
                    Error = new ResultBaseModel { IsSuccess = true }
                };
            else
                return new CompanyTransactionHistoryShortModel();
        }

        #region Company form
        [HttpGet("getcompanyform"), Authorize(Roles = "pestunov")]
        public async Task<FormCompanyModel> GetCompanyForm(long? id)
        {
            var industries = await unitOfWork.Industry.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
            var sectors = await unitOfWork.Sector.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
            var model = new FormCompanyModel { Industries = industries, Sectors = sectors };

            if (id.HasValue)
            {
                var company = await unitOfWork.Company.FindByIdAsync(id.Value).ConfigureAwait(false);
                if (company != null)
                {
                    model.Id = company.Id;
                    model.Name = company.Name;
                    model.DateSplit = company.DateSplit;
                    model.IndustryId = $"{company.IndustryId}";
                    model.SectorId = $"{company.SectorId}";

                    return model;
                }
            }

            model.SectorId = sectors.First().Id.ToString();
            model.IndustryId = industries.First().Id.ToString();

            return model;
        }
        [HttpPost("setcompany"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SetCompany([FromBody] FormCompanyModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { "Model state invalid." } });

            Company company;
            if (model.Id.HasValue)
            {
                company = await unitOfWork.Company.FindByIdAsync(model.Id.Value).ConfigureAwait(false);
                if (company != null)
                {
                    company.Name = model.Name;
                    company.DateSplit = model.DateSplit;
                    company.DateUpdate = DateTime.Now;
                    company.IndustryId = long.Parse(model.IndustryId);
                    company.SectorId = long.Parse(model.SectorId);
                }
                else
                    return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { "Company not found." } });
            }
            else
            {
                var companies = unitOfWork.Company.GetAll().Select(x => x.Name);
                if (companies.Contains(model.Name))
                    return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { $"Company {model.Name} allready." } });

                company = new Company
                {
                    Name = model.Name,
                    DateSplit = model.DateSplit,
                    IndustryId = long.Parse(model.IndustryId),
                    SectorId = long.Parse(model.SectorId)
                };

                await unitOfWork.Company.CreateEntityAsync(company).ConfigureAwait(false);
            }

            if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                return Ok(new CompanyResult { IsSuccess = true, CompanyId = company.Id });
            return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { "Saving error." } });
        }

        [HttpGet("gettickerforms"), Authorize(Roles = "pestunov")]
        public async Task<List<FormTickerModel>> GetTickerForms(long id)
        {
            var exchanges = await unitOfWork.Exchange.GetAll().OrderBy(x => x.Name).Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
            var lots = await unitOfWork.Lot.GetAll().OrderBy(x => x.Value).Select(x => new ViewModelBase { Id = x.Id, Name = x.Value.ToString() }).ToListAsync().ConfigureAwait(false);
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);

            if (company != null)
            {
                var tickers = company.Tickers;
                if (tickers != null && tickers.Any())
                {
                    return tickers.Select(x => new FormTickerModel
                    {
                        Id = x.Id,
                        CompanyId = x.CompanyId,
                        Name = x.Name,
                        Exchanges = exchanges,
                        ExcangeId = x.ExchangeId.ToString(),
                        Lots = lots,
                        LotId = x.LotId.ToString()
                    }).ToList();
                }
            }

            return new List<FormTickerModel>(){ new FormTickerModel
            {
                CompanyId = id,
                Exchanges = exchanges,
                ExcangeId = exchanges.First().Id.ToString(),
                Lots = lots,
                LotId = lots.First().Id.ToString()
            }};
        }
        [HttpPost("setticker"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SetTicker([FromBody] FormTickerModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { "Model state invalid." } });

            Ticker ticker;
            if (model.Id.HasValue)
            {
                ticker = await unitOfWork.Ticker.FindByIdAsync(model.Id.Value).ConfigureAwait(false);
                if (ticker != null)
                {
                    ticker.Name = ticker.Name;
                    ticker.DateUpdate = DateTime.Now;
                    ticker.ExchangeId = long.Parse(model.ExcangeId);
                    ticker.LotId = long.Parse(model.LotId);

                }
                else
                    return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { $"{ticker.Name} not found." } });
            }
            else
            {
                var tickers = unitOfWork.Ticker.GetAll().Select(x => x.Name);
                if (tickers.Contains(model.Name))
                    return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { $"Ticker {model.Name} allready." } });

                ticker = new Ticker
                {
                    CompanyId = model.CompanyId,
                    Name = model.Name,
                    ExchangeId = long.Parse(model.ExcangeId),
                    LotId = long.Parse(model.LotId)
                };

                await unitOfWork.Ticker.CreateEntityAsync(ticker);
            }

            if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                return Ok(new ResultBaseModel { IsSuccess = true });
            else
                return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { "Saving error." } });
        }

        [HttpGet("getisinforms"), Authorize(Roles = "pestunov")]
        public async Task<List<FormIsinModel>> GetIsinsForm(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            if (company != null)
            {
                var isins = company.Isins;
                if (isins != null && isins.Any())
                    return isins.Select(x => new FormIsinModel { Id = x.Id, CompanyId = x.CompanyId, Name = x.Name, }).ToList();
            }

            return new List<FormIsinModel> { new FormIsinModel { CompanyId = id } };
        }
        [HttpPost("setisin"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SetIsin([FromBody] FormIsinModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { "Model state invalid." } });
            if (string.IsNullOrWhiteSpace(model.Name))
                return Ok(new ResultBaseModel { IsSuccess = true });

            Isin isin;
            if (model.Id.HasValue)
            {
                isin = await unitOfWork.Isin.FindByIdAsync(model.Id.Value).ConfigureAwait(false);
                if (isin != null)
                {
                    isin.Name = model.Name;
                    isin.DateUpdate = DateTime.Now;
                }
                else
                    return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { $"{model.Name} not found." } });
            }
            else
            {
                var isins = unitOfWork.Isin.GetAll().Select(x => x.Name);
                if (isins.Contains(model.Name))
                    return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { $"Isin {model.Name} allready." } });
                isin = new Isin { CompanyId = model.CompanyId, Name = model.Name };
                await unitOfWork.Isin.CreateEntityAsync(isin).ConfigureAwait(false);
            }

            if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                return Ok(new ResultBaseModel { IsSuccess = true });
            else
                return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { "Saving error." } });
        }

        [HttpGet("getreportsourceform"), Authorize(Roles = "pestunov")]
        public async Task<FormReportSourceModel> GetReportSourceForm(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            if (company != null)
            {
                var reportSource = company.ReportSource;
                if (reportSource != null)
                    return new FormReportSourceModel
                    {
                        Id = reportSource.Id,
                        CompanyId = reportSource.CompanyId,
                        Key = reportSource.Key,
                        Value = reportSource.Value
                    };
            }

            return new FormReportSourceModel { CompanyId = id };
        }
        [HttpPost("setreportsource"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SetReportSource([FromBody] FormReportSourceModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { "Model state invalid." } });

            ReportSource reportSource;
            if (model.Id.HasValue)
            {
                reportSource = await unitOfWork.ReportSource.FindByIdAsync(model.Id.Value).ConfigureAwait(false);
                if (reportSource != null)
                {
                    reportSource.Key = model.Key;
                    reportSource.Value = model.Value;
                    reportSource.DateUpdate = DateTime.Now;
                }
                else
                    return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { $"Report source {model.Value} not found." } });
            }
            else
            {
                var reportSources = unitOfWork.ReportSource.GetAll().Select(x => x.Value);
                if (reportSources.Contains(model.Value))
                    return BadRequest(new CompanyResult { IsSuccess = false, Errors = new string[] { $"Report source {model.Value} allready." } });

                reportSource = new ReportSource
                {
                    CompanyId = model.CompanyId,
                    Key = model.Key,
                    Value = model.Value
                };
                await unitOfWork.ReportSource.CreateEntityAsync(reportSource).ConfigureAwait(false);
            }

            if (!(await unitOfWork.CompleteAsync().ConfigureAwait(false) < 0))
                return Ok(new ResultBaseModel { IsSuccess = true });
            else
                return BadRequest(new ResultBaseModel { IsSuccess = false, Errors = new string[] { "Saving error." } });
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
