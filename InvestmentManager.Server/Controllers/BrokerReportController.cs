using InvestmentManager.BrokerService;
using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;
using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.EntityViewModels;
using InvestmentManager.ViewModels.ReportModels.BrokerReportModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class BrokerReportController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMemoryCache memoryCache;
        private readonly IInvestBrokerService brokerService;
        private readonly IInvestMapper portfolioMapper;

        public BrokerReportController(
            IUnitOfWorkFactory unitOfWork
            , UserManager<IdentityUser> userManager
            , IMemoryCache memoryCache
            , IInvestBrokerService brokerService
            , IInvestMapper portfolioMapper)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.memoryCache = memoryCache;
            this.brokerService = brokerService;
            this.portfolioMapper = portfolioMapper;
        }

        [HttpPost("parsebcs"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> ParseBcsReports()
        {
            var files = HttpContext.Request.Form.Files;
            string userId = userManager.GetUserId(User);
            try
            {
                var parsedReports = await brokerService.GetNewReportsAsync(files, userId).ConfigureAwait(false);
                var result = portfolioMapper.MapBcsReports(parsedReports);
                foreach (var entityReportModel in parsedReports.Reports)
                    memoryCache.Set(entityReportModel.AccountName, entityReportModel, TimeSpan.FromMinutes(5));
                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }
        [HttpPost("save"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> SeveBrokerReports([FromBody] string accountId)
        {
            var account = unitOfWork.Account.GetAll().FirstOrDefault(x => x.Name.Equals(accountId));
            if (account != null && memoryCache.TryGetValue(accountId, out EntityReportModel saveResult))
            {
                if (saveResult.AccountTransactions.Any())
                    await unitOfWork.AccountTransaction.CreateEntitiesAsync(saveResult.AccountTransactions).ConfigureAwait(false);

                if (saveResult.StockTransactions.Any())
                    await unitOfWork.StockTransaction.CreateEntitiesAsync(saveResult.StockTransactions).ConfigureAwait(false);

                if (saveResult.Dividends.Any())
                    await unitOfWork.Dividend.CreateEntitiesAsync(saveResult.Dividends).ConfigureAwait(false);

                if (saveResult.Comissions.Any())
                    await unitOfWork.Comission.CreateEntitiesAsync(saveResult.Comissions).ConfigureAwait(false);

                if (saveResult.ExchangeRates.Any())
                    await unitOfWork.ExchangeRate.CreateEntitiesAsync(saveResult.ExchangeRates).ConfigureAwait(false);

                try
                {
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }


        [HttpGet("stocktransactionerrordata")]
        public async Task<StockTransactionErrorForm> GetStockTransactionErrorData()
        {
            if (!memoryCache.TryGetValue("companies", out List<ViewModelBase> companies))
            {
                companies = await unitOfWork.Company.GetAll().Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("companies", companies, TimeSpan.FromSeconds(30));
            }
            if (!memoryCache.TryGetValue("lots", out List<LotModel> lots))
            {
                lots = await unitOfWork.Lot.GetAll().Select(x => new LotModel { Id = x.Id, Value = x.Value }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("lots", lots, TimeSpan.FromSeconds(30));
            }
            if (!memoryCache.TryGetValue("exchanges", out List<ViewModelBase> exchanges))
            {
                exchanges = await unitOfWork.Exchange.GetAll().Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("exchanges", exchanges, TimeSpan.FromSeconds(30));
            }

            return new StockTransactionErrorForm
            {
                Companies = companies,
                Lots = lots,
                Exchanges = exchanges
            };
        }
        [HttpPost("setticker")]
        public async Task<IActionResult> SetTicker([FromBody] StockTransactionErrorResultModel model)
        {
            if (
                long.TryParse(model.CompanyId, out long companyId)
                && long.TryParse(model.LotId, out long lotId)
                && long.TryParse(model.ExchangeId, out long exchangeId)
                )
            {
                await unitOfWork.Ticker.CreateEntityAsync(new Ticker
                {
                    Name = model.TikerName,
                    CompanyId = companyId,
                    ExchangeId = exchangeId,
                    LotId = lotId
                }).ConfigureAwait(false);

                try
                {
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpGet("dividenderrordata")]
        public async Task<DividendErrorForm> GetDividendErrorData()
        {
            var result = new DividendErrorForm();
            if (memoryCache.TryGetValue("brCompanies", out List<ViewModelBase> companyModels))
                result.Companies = companyModels;
            else
            {
                result.Companies = await unitOfWork.Company.GetAll().Select(x => new ViewModelBase { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
                memoryCache.Set("brCompanies", result.Companies, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }
            return result;
        }
        [HttpPost("setisin")]
        public async Task<IActionResult> SetIsisn([FromBody] DividendErrorResultModel model)
        {
            if (!memoryCache.TryGetValue(model.IdentifierName, out string _))
                memoryCache.Set(model.IdentifierName, model.IdentifierName, TimeSpan.FromMinutes(1));
            else
                return Ok();

            if (long.TryParse(model.CompanyId, out long companyId))
            {
                if (unitOfWork.Isin.GetAll().Where(x => x.CompanyId == companyId && x.Name.Equals(model.IdentifierName)).Any())
                    return Ok();

                await unitOfWork.Isin.CreateEntityAsync(new Isin
                {
                    Name = model.IdentifierName,
                    CompanyId = companyId
                }).ConfigureAwait(false);

                try
                {
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }

        [HttpPost("setaccount")]
        public async Task<IActionResult> SetAccountError([FromBody] AccountErrorForm model)
        {
            if (!string.IsNullOrWhiteSpace(model.AccountName))
            {
                await unitOfWork.Account.CreateEntityAsync(new Account
                {
                    Name = model.AccountName,
                    UserId = userManager.GetUserId(User)
                }).ConfigureAwait(false);

                try
                {
                    await unitOfWork.CompleteAsync().ConfigureAwait(false);
                    return Ok();
                }
                catch
                {
                    return BadRequest();
                }
            }
            return BadRequest();
        }
    }
}
