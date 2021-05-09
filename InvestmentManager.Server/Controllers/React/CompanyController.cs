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


        [HttpGet("{id}/buyrecommendation/")]
        public async Task<ClientBaseResponse<ClientCompanyBuyRecommendation>> GetRecommendationForBuy(long id) => new();
        [HttpGet("{id}/sellrecommendation/")]
        public async Task<ClientBaseResponse<ClientCompanySellRecommendation>> GetRecommendationForSale(long id) => new();
        [HttpGet("{id}/rating/")]
        public async Task<ClientBaseResponse<ClientCompanyRating>> GetRating(long id) => new();


        [HttpGet("{id}/summary/{accountId}")]
        public async Task<ClientBaseResponse<ClientCompanySummary>> GetSummary(long id, long accountId)
        {
            var result = new ClientBaseResponse<ClientCompanySummary>();

            var company = await unitOfWork.Company.FindByIdAsync(id);
            if (company is null)
            {
                result.Errors = new[] { "company not gound" };
                return result;
            }

            var companySummary = await unitOfWork.CompanySummary.GetAll().FirstOrDefaultAsync(x => x.AccountId == accountId && x.CompanyId == id);
            var companyDividendSummary = await unitOfWork.DividendSummary.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id && x.AccountId == accountId);
            var companyRating = await unitOfWork.Rating.GetAll().FirstOrDefaultAsync(x => x.CompanyId == id);

            var summary = new ClientCompanySummary();

            if (company is not null)
            {
                summary.Industry = company.Industry.Name;
                summary.Sector = company.Sector.Name;
                summary.Currency = catalogService.GetCurrencyName(company.Tickers.FirstOrDefault().Prices.FirstOrDefault().CurrencyId);
            }
            if (companySummary is not null)
            {
                summary.ActualLot = companySummary.ActualLot;
                summary.CurrentProfit = companySummary.CurrentProfit;
            }
            if (companyDividendSummary is not null)
            {
                summary.DividendSum = companyDividendSummary.TotalSum;
            }
            if (companyRating is not null)
            {
                int ratingCount = await unitOfWork.Rating.GetAll().CountAsync();
                summary.RatingPlace = $"{companyRating.Place} of {ratingCount}";
            }

            result.IsSuccess = true;
            result.Data = summary;

            return result;
        }


        [HttpGet("{id}/transactions/{accountId}/")]
        public async Task<ClientBaseResponse<PaginationModel<ClientCompanyTransaction>>> GetTransactions(long id, long accountId, int page = 1, int limit = 5)
        {
            var result = new ClientBaseResponse<PaginationModel<ClientCompanyTransaction>>();

            var query = unitOfWork.StockTransaction.GetAll().Where(x => x.AccountId == accountId && x.Ticker.CompanyId == id).OrderByDescending(x => x.DateOperation);
            int totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(x => new ClientCompanyTransaction
                {
                    AccountId = x.AccountId,
                    Cost = x.Cost,
                    CurrencyId = x.CurrencyId,
                    DateOperation = x.DateOperation,
                    ExchangeId = x.ExchangeId,
                    Identifier = x.Identifier,
                    Quantity = x.Quantity,
                    Status = x.TransactionStatus.Name,
                    StatusId = x.TransactionStatusId,
                    TickerId = x.TickerId
                })
                .ToArrayAsync();

            return new() { IsSuccess = true, Data = new() { Items = items, TotalCount = totalCount } };
        }
        [HttpGet("{id}/dividends/{accountId}")]
        public async Task<ClientBaseResponse<PaginationModel<ClientCompanyDividend>>> GetDividends(long id, long accountId, int page = 1, int limit = 5) => new();
        [HttpGet("{id}/prices/{accountId}")]
        public async Task<ClientBaseResponse<PaginationModel<ClientCompanyPrice>>> GetPrices(long id, long accountId, int page = 1, int limit = 5) => new();
        [HttpGet("{id}/indexes/{accountId}")]
        public async Task<ClientBaseResponse<PaginationModel<ClientCompanyIndex>>> GetInexes(long id, long accountId, int page = 1, int limit = 5) => new();
        [HttpGet("{id}/reports/{accountId}")]
        public async Task<ClientBaseResponse<PaginationModel<ClientCompanyReport>>> GetReports(long id, long accountId, int page = 1, int limit = 5) => new();
    }
}
