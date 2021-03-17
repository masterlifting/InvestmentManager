using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IReckonerService reckonerService;

        public ExchangeRatesController(
            IBaseRestMethod restMethod
            , IUnitOfWorkFactory unitOfWork
            , IReckonerService reckonerService)
        {
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
            this.reckonerService = reckonerService;
        }

        [HttpGet("byaccountid/{id}")]
        public async Task<IActionResult> GetByAccountId(long id)
        {
            var rates = (await unitOfWork.Account.FindByIdAsync(id))?.ExchangeRates;

            return rates is null
                ? NoContent()
                : Ok(rates.Select(x => new ExchangeRateModel
                {
                    DateOperation = x.DateOperation,
                    StatusName = x.TransactionStatus.Name,
                    Rate = x.Rate,
                    Quantity = x.Quantity
                }).ToList());
        }
        [HttpGet("byaccountid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountId(long id)
        {
            var rates = (await unitOfWork.Account.FindByIdAsync(id))?.ExchangeRates;
            var summary = await unitOfWork.ExchangeRateSummary.GetAll().Where(x => x.AccountId == id).Join(unitOfWork.Currency.GetAll(), x => x.CurrencyId, y => y.Id, (x, y) => new
            {
                x.AvgPurchasedRate,
                x.AvgSoldRate,
                Currency = y.Name
            }).ToListAsync();

            if (rates is null || !rates.Any())
                return NoContent();

            var lastRate = rates.OrderBy(x => x.DateOperation).Last();

            return Ok(new SummaryExchangeRate
            {
                DateLastOperation = lastRate.DateOperation,
                Rate = lastRate.Rate,
                StatusName = lastRate.TransactionStatus.Name,
                Details = summary.GroupBy(x => x.Currency).Select(x => new SummaryExchangeRateDetail
                {
                    Currency = x.Key,
                    AvgPurchasedRate = x.Average(y => y.AvgPurchasedRate),
                    AvgSoldRate = x.Average(y => y.AvgSoldRate)
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(ExchangeRateModel model)
        {
            var entity = new ExchangeRate
            {
                AccountId = model.AccountId,
                CurrencyId = model.CurrencyId,
                TransactionStatusId = model.StatusId,
                DateOperation = model.DateOperation,
                Identifier = model.Identifier,
                Quantity = model.Quantity,
                Rate = model.Rate
            };

            async Task<bool> ExchangeRateValidatorAsync(ExchangeRateModel model)
            {
                if (model.StatusId == (long)TransactionStatusTypes.Sell)
                {
                    var summary = await unitOfWork.AccountSummary.GetAll()
                        .FirstOrDefaultAsync(x =>
                        x.AccountId == model.AccountId
                        && x.CurrencyId == model.CurrencyId)
                        ;

                    return summary is not null && model.Quantity <= summary.FreeSum;
                }
                else if (model.StatusId == (long)TransactionStatusTypes.Buy)
                {
                    var summary = await unitOfWork.AccountSummary.GetAll()
                        .FirstOrDefaultAsync(x =>
                        x.AccountId == model.AccountId
                        && x.CurrencyId == (long)CurrencyTypes.rub)
                        ;

                    return summary is not null && model.Quantity * model.Rate <= summary.FreeSum;
                }
                else
                    return true;
            }

            var result = await restMethod.BasePostAsync(ModelState, entity, model, ExchangeRateValidatorAsync);

            if (result.IsSuccess)
            {
                result.Info += await reckonerService.UpgradeByExchangeRateChangeAsync(entity) ? " Recalculated" : " NOT Recalculated.";
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
