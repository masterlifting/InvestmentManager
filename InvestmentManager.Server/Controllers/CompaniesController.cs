using InvestmentManager.Repository;
using InvestmentManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using InvestmentManager.Entities.Market;
using System;
using InvestmentManager.Server.RestServices;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Services.Interfaces;
using Microsoft.Extensions.Configuration;

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
            Ok(await unitOfWork.Company.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false));
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            return company is null ? NoContent() : Ok(new CompanyModel
            {
                Id = company.Id,
                Name = company.Name,
                DateSplit = company.DateSplit,
                IndustryId = company.IndustryId,
                SectorId = company.SectorId
            });
        }
        [HttpGet("bycompanyid/{companyId}/byaccountid/{accountId}/additional/")]
        public async Task<IActionResult> GetAdditional(long companyId, long accountId)
        {
            var company = await unitOfWork.Company.FindByIdAsync(companyId).ConfigureAwait(false);
            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CompanyId == companyId).ConfigureAwait(false);
            return company is null || summary is null ? NoContent() : Ok(new CompanyAdditionalModel
            {
                IndustryName = company.Industry.Name,
                SectorName = company.Sector.Name,
                Currency = catalogService.GetCurrencyName(summary.CurrencyId),
                ActualLot = summary.ActualLot,
                CurrentProfit = summary.CurrentProfit
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
                .ToListAsync().ConfigureAwait(false);

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(await companies.CountAsync().ConfigureAwait(false), value, pageSize);
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
                var names = await unitOfWork.Company.GetAll().Select(x => x.Name).ToListAsync().ConfigureAwait(false);
                return !names.Where(x => x.Equals(model.Name, StringComparison.OrdinalIgnoreCase)).Any();
            }

            var result = await restMethod.BasePostAsync(ModelState, entity, model, CompanyValidatorAsync).ConfigureAwait(false);
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
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
            return result.IsSuccess ? (IActionResult)Ok(result) : BadRequest(result);
        }
    }
}
