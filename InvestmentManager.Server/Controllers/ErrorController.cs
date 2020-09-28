using InvestmentManager.DomainModels;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;
using InvestmentManager.Repository;
using InvestmentManager.ViewModels.PortfolioModels.BrokerReportErrorModels;
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
    [ApiController]
    [Authorize]
    [Route("[controller]")]

    public class ErrorController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IMemoryCache memoryCache;

        public ErrorController(
            IUnitOfWorkFactory unitOfWork
            , UserManager<IdentityUser> userManager
            , IMemoryCache memoryCache)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.memoryCache = memoryCache;
        }

        [Route("stocktransactionerror")]
        [HttpGet]
        public async Task<StockTransactionErrorModel> GetStockTransactionError()
        {
            var result = new StockTransactionErrorModel();
            if (memoryCache.TryGetValue(nameof(CompanyD), out List<CompanyD> companyDs))
                result.Companies = companyDs;
            else
            {
                result.Companies = await unitOfWork.Company.GetAll().Select(x => new CompanyD { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
                memoryCache.Set(nameof(CompanyD), result.Companies, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            if (memoryCache.TryGetValue(nameof(LotD), out List<LotD> lotDs))
                result.Lots = lotDs;
            else
            {
                result.Lots = await unitOfWork.Lot.GetAll().Select(x => new LotD { Id = x.Id, Value = x.Value }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set(nameof(LotD), result.Lots, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            if (memoryCache.TryGetValue(nameof(ExchangeD), out List<ExchangeD> exchangeDs))
                result.Exchanges = exchangeDs;
            else
            {
                result.Exchanges = await unitOfWork.Exchange.GetAll().Select(x => new ExchangeD { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
                memoryCache.Set(nameof(ExchangeD), result.Exchanges, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }

            return result;
        }
        [Route("stocktransactionerror")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StockTransactionErrorResultModel model)
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

        [Route("dividenderror")]
        [HttpGet]
        public async Task<DividendErrorModel> GetDividendError()
        {
            var result = new DividendErrorModel();
            if (memoryCache.TryGetValue(nameof(CompanyD), out List<CompanyD> companyDs))
                result.Companies = companyDs;
            else
            {
                result.Companies = await unitOfWork.Company.GetAll().Select(x => new CompanyD { Id = x.Id, Name = x.Name }).OrderBy(x => x.Name).ToListAsync().ConfigureAwait(false);
                memoryCache.Set(nameof(CompanyD), result.Companies, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            }
            return result;
        }

        [Route("dividenderror")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DividendErrorResultModel model)
        {
            if (memoryCache.TryGetValue(model.IdentifierName, out string _))
                return Ok();
            else
                memoryCache.Set(model.IdentifierName, model.IdentifierName, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

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

        [Route("accounterror")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AccountErrorResultModel model)
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
