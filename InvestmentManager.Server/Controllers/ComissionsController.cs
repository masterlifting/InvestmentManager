﻿using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class ComissionsController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IReckonerService reckonerService;

        public ComissionsController(
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
            var comissions = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.Comissions;

            return comissions is null
                ? NoContent()
                : Ok(comissions.Select(x => new ComissionModel
                {
                    DateOperation = x.DateOperation,
                    TypeName = x.ComissionType.Name,
                    Amount = x.Amount
                }).ToList());
        }
        [HttpGet("byaccountid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByAccountId(long id)
        {
            var comissionss = (await unitOfWork.Account.FindByIdAsync(id).ConfigureAwait(false))?.Comissions;

            if (comissionss is null || !comissionss.Any())
                return NoContent();

            var targetComissions = comissionss.OrderBy(x => x.DateOperation);

            return Ok(new SummaryComission
            {
                DateFirstComission = targetComissions.First().DateOperation,
                DateLastComission = targetComissions.Last().DateOperation,
                Amount = targetComissions.Sum(x => x.Amount),
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(ComissionModel model)
        {
            var entity = new Comission
            {
                AccountId = model.AccountId,
                CurrencyId = model.CurrencyId,
                ComissionTypeId = model.TypeId,
                Amount = model.Amount,
                DateOperation = model.DateOperation
            };

            var result = await restMethod.BasePostAsync(ModelState, entity, model).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                await reckonerService.UpgradeByComissionChangeAsync(entity).ConfigureAwait(false);
                return Ok(result);
            }
            else
                return BadRequest(result);
        }
    }
}
