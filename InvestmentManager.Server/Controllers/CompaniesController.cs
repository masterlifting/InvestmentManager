using InvestmentManager.Entities.Market;
using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IBaseRestMethod restMethod;
        private readonly ICatalogService catalogService;
        private readonly IConfiguration configuration;

        public CompaniesController(
            IUnitOfWorkFactory unitOfWork
            , IBaseRestMethod restMethod
            , ICatalogService catalogService
            , IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.restMethod = restMethod;
            this.catalogService = catalogService;
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await unitOfWork.Company.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id);
            return company is null ? NoContent() : Ok(new CompanyModel
            {
                Id = company.Id,
                Name = company.Name,
                DateSplit = company.DateSplit,
                IndustryId = company.IndustryId,
                SectorId = company.SectorId
            });
        }
        [HttpGet("{id}/additional/")]
        public async Task<IActionResult> GetAdditional(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id);

            return company is null ? NoContent() : Ok(new CompanyAdditionalModel
            {
                IndustryName = company.Industry.Name,
                SectorName = company.Sector.Name,
                Currency = catalogService.GetCurrencyName(company.Tickers.FirstOrDefault().Prices.FirstOrDefault().CurrencyId),
            });
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = int.Parse(configuration["PaginationPageSize"]);

            var companies = unitOfWork.Company.GetAll().OrderBy(x => x.Name);
            var items = await companies
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ShortView { Id = x.Id, Name = x.Name, Description = x.Tickers.FirstOrDefault().Name })
                .ToListAsync();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(await companies.CountAsync(), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);
        }

        [HttpPost, Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Post(CompanyModel model)
        {
            var entity = new Company
            {
                Name = model.Name,
                DateSplit = model.DateSplit,
                IndustryId = model.IndustryId,
                SectorId = model.SectorId
            };
            async Task<bool> CompanyValidatorAsync(CompanyModel model)
            {
                var names = await unitOfWork.Company.GetAll().Select(x => x.Name).ToListAsync();
                return !names.Where(x => x.Equals(model.Name, StringComparison.OrdinalIgnoreCase)).Any();
            }

            var result = await restMethod.BasePostAsync(ModelState, entity, model, CompanyValidatorAsync);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}"), Authorize(Roles = "pestunov")]
        public async Task<IActionResult> Put(long id, CompanyModel model)
        {
            void UpdateCompany(Company company)
            {
                company.DateSplit = model.DateSplit;
                company.DateUpdate = DateTime.Now;
                company.IndustryId = model.IndustryId;
                company.Name = model.Name;
                company.SectorId = model.SectorId;
            }

            var result = await restMethod.BasePutAsync<Company>(ModelState, id, UpdateCompany);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
