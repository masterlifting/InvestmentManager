using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{

    [ApiController, Route("[controller]")]
    public class SectorsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public SectorsController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Sector.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false));
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var sector = await unitOfWork.Sector.FindByIdAsync(id).ConfigureAwait(false);
            return sector is null ? NoContent() : Ok(new ShortView { Id = sector.Id, Name = sector.Name });
        }
    }
}
