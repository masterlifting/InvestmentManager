using InvestmentManager.ClientModels;
using InvestmentManager.ClientModels.CompanyModels;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly ICatalogService catalogService;

        public CompanyController(IUnitOfWorkFactory unitOfWork, ICatalogService catalogService)
        {
            this.unitOfWork = unitOfWork;
            this.catalogService = catalogService;
        }
        [HttpGet("{id}")]
        public async Task<ClientBaseResponse<ClientCompany>> Get(long id)
        {
            var result = new ClientBaseResponse<ClientCompany>();

            var company = await unitOfWork.Company.FindByIdAsync(id);

            if (company is null)
            {
                result.Errors = new[] { "company not found" };
                return result;
            }

            result.IsSuccess = true;
            result.Data = new()
            {
                Id = company.Id,
                Name = company.Name,
                DateSplit = company.DateSplit,
                IndustryId = company.IndustryId,
                SectorId = company.SectorId,
                Description = company.Tickers.FirstOrDefault().Name
            };


            return result;
        }
        public async Task<ClientBaseResponse<PaginationModel<ClientCompany>>> Get(int page = 1, int limit = 10, string phrase = null)
        {
            var result = new ClientBaseResponse<PaginationModel<ClientCompany>>();

            var query = unitOfWork.Company.GetAll();

            if (!string.IsNullOrWhiteSpace(phrase))
            {
                page = 1;
                query = query.Where(x => x.Name.ToLower().Contains(phrase.ToLower()));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(x => new ClientCompany
                {
                    Id = x.Id,
                    Name = x.Name,
                    DateSplit = x.DateSplit,
                    IndustryId = x.IndustryId,
                    SectorId = x.SectorId,
                    Description = x.Tickers.FirstOrDefault().Name
                })
                .ToArrayAsync();

            return new() { IsSuccess = true, Data = new() { Items = items, TotalCount = totalCount } };
        }

        [HttpGet("{id}/additional/")]
        public async Task<ClientBaseResponse<ClientCompanyAdditional>> GetAdditional(long id)
        {
            var result = new ClientBaseResponse<ClientCompanyAdditional>();

            var company = await unitOfWork.Company.FindByIdAsync(id);

            if (company is null)
            {
                result.Errors = new[] { "company not found" };
                return result;
            }

            result.IsSuccess = true;
            result.Data = new()
            {
                Industry = company.Industry.Name,
                Sector = company.Sector.Name,
                Currency = catalogService.GetCurrencyName(company.Tickers.FirstOrDefault().Prices.FirstOrDefault().CurrencyId),
            };

            return result;
        }

        [HttpGet("{id}/transactions/{accountId}/summary/")]
        public async Task<ClientBaseResponse<ClientCompanyTransactionsSummary>> GetTransactionsSummary(long id, long accountId)
        {
            var result = new ClientBaseResponse<ClientCompanyTransactionsSummary>();

            var lastTransaction = await unitOfWork.StockTransaction.GetAll()
               .Where(x => x.AccountId == accountId && x.Ticker.CompanyId == id)
               .OrderBy(x => x.DateOperation)
               .LastOrDefaultAsync();

            if (lastTransaction is null)
            {
                result.Errors = new[] { "last transactions not found" };
                return result;
            }

            var summary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CompanyId == id);

            result.IsSuccess = true;
            result.Data = new()
            {
                Date = lastTransaction.DateOperation,
                Status = catalogService.GetStatusName(lastTransaction.TransactionStatusId),
                Quantity = lastTransaction.Quantity,
                Cost = lastTransaction.Cost,
                ActualLot = summary?.ActualLot ?? 0,
                CurrentProfit = summary?.CurrentProfit ?? 0
            };

            return result;
        }
    }
}
