using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{

    [ApiController, Route("[controller]")]
    public class SectorsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConfiguration configuration;

        public SectorsController(IUnitOfWorkFactory unitOfWork, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = int.Parse(configuration["PaginationPageSize"]);

            var companies = unitOfWork.Company.GetAll();
            var sectors = unitOfWork.Sector.GetAll();

            if (companies is null || sectors is null)
                return NoContent();

            var result = await companies.Join(sectors, x => x.SectorId, y => y.Id, (x, y) => new ShortView 
            { 
                Id = x.Id, 
                Name = x.Name, 
                Description = y.Name 
            }).OrderBy(x => x.Description)
            .ToListAsync();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Items = result.Skip((value - 1) * pageSize).Take(pageSize).ToList();
            paginationResult.Pagination.SetPagination(result.Count, value, pageSize);

            return Ok(paginationResult);
        }
        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Sector.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var sector = await unitOfWork.Sector.FindByIdAsync(id);
            return sector is null ? NoContent() : Ok(new ShortView { Id = sector.Id, Name = sector.Name });
        }
    }
}
