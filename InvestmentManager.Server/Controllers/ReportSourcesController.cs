using InvestmentManager.Entities.Market;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ReportSourcesController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;

        public ReportSourcesController(IBaseRestMethod restMethod, IUnitOfWorkFactory unitOfWork)
        {
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
        }

        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var reportSource = (await unitOfWork.Company.FindByIdAsync(id))?.ReportSource;

            return reportSource is null ? NoContent() : Ok(new ReportSourceModel
            {
                Id = reportSource.Id,
                CompanyId = reportSource.CompanyId,
                Key = reportSource.Key,
                Value = reportSource.Value
            });
        }

        [HttpPost, Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Post(ReportSourceModel model)
        {
            var entity = new ReportSource { CompanyId = model.CompanyId, Key = model.Key, Value = model.Value };
            async Task<bool> ReportSourceValidatorAsync(ReportSourceModel model)
            {
                string value = (await unitOfWork.ReportSource.GetAll().FirstOrDefaultAsync(x => x.CompanyId == model.CompanyId))?.Value;
                return value is not null ? !value.Equals(model.Value, StringComparison.OrdinalIgnoreCase) : true;
            }

            var result = await restMethod.BasePostAsync(ModelState, entity, model, ReportSourceValidatorAsync);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, ReportSourceModel model)
        {
            void UpdateReportSource(ReportSource isin)
            {
                isin.DateUpdate = DateTime.Now;
                isin.Key = model.Key;
                isin.Value = model.Value;
            }

            var result = await restMethod.BasePutAsync<ReportSource>(ModelState, id, UpdateReportSource);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
