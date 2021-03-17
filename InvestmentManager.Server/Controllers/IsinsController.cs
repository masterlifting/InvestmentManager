using InvestmentManager.Entities.Market;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class IsinsController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;

        public IsinsController(IBaseRestMethod restMethod, IUnitOfWorkFactory unitOfWork)
        {
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Isin.GetAll().Select(x => new IsinModel
            {
                Id = x.Id,
                Name = x.Name,
                CompanyId = x.CompanyId
            }).ToListAsync());
        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var isins = (await unitOfWork.Company.FindByIdAsync(id))?.Isins;
            return isins is null
                ? NoContent()
                : Ok(isins.Select(x => new IsinModel { Id = x.Id, CompanyId = x.CompanyId, Name = x.Name }).ToList());
        }
        [HttpPost, Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Post(IsinModel model)
        {
            var entity = new Isin { Name = model.Name, CompanyId = model.CompanyId };

            async Task<bool> IsinValidatorAsync(IsinModel model) =>
                !await unitOfWork.Isin.GetAll().Where(x => x.Name.Equals(model.Name)).AnyAsync();

            var result = await restMethod.BasePostAsync(ModelState, entity, model, IsinValidatorAsync);

            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, IsinModel model)
        {
            void UpdateIsin(Isin isin)
            {
                isin.DateUpdate = DateTime.Now;
                isin.Name = model.Name;
            }

            var result = await restMethod.BasePutAsync<Isin>(ModelState, id, UpdateIsin);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
        [HttpDelete("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await restMethod.BaseDeleteAsync<Isin>(id);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
