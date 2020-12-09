using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class IndustriesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConfiguration configuration;

        public IndustriesController(IUnitOfWorkFactory unitOfWork, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = int.Parse(configuration["PaginationPageSize"]);

            var companies = unitOfWork.Company.GetAll();
            var industries = unitOfWork.Industry.GetAll().OrderBy(x => x.Name);

            var result = industries?.Join(companies, x => x.Id, y => y.IndustryId, (x, y) => new ShortView { Id = y.Id, Name = y.Name, Description = x.Name });

            if (result is null)
                return NoContent();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Items = await result.Skip((value - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
            paginationResult.Pagination.SetPagination(await result.CountAsync().ConfigureAwait(false), value, pageSize);

            return Ok(paginationResult);
        }
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
