using InvestmentManager.Entities.Broker;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize(Roles = "pestunov")]
    public class ComissionTypesController : ControllerBase
    {
        private readonly IBaseRestMethod restMethod;
        private readonly IUnitOfWorkFactory unitOfWork;

        public ComissionTypesController(
            IBaseRestMethod restMethod
            , IUnitOfWorkFactory unitOfWork)
        {
            this.restMethod = restMethod;
            this.unitOfWork = unitOfWork;
        }
        [HttpPost]
        public async Task<IActionResult> Post(ComissionTypeModel model)
        {
            var entity = new ComissionType { Name = model.Name };

            async Task<bool> ComissionTypeValidatorAsync(ComissionTypeModel model)
            {
                var names = await unitOfWork.ComissionType.GetAll().Select(x => x.Name).ToListAsync();
                return !names.Where(x => x.Equals(model.Name, StringComparison.OrdinalIgnoreCase)).Any();
            }

            var result = await restMethod.BasePostAsync(ModelState, entity, model, ComissionTypeValidatorAsync);

            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
