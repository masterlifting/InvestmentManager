using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class IndustriesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public IndustriesController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;
        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Industry.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false));
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var industry = await unitOfWork.Industry.FindByIdAsync(id).ConfigureAwait(false);
            return industry is null ? NoContent() : Ok(new ShortView { Id = industry.Id, Name = industry.Name });
        }
    }
}
