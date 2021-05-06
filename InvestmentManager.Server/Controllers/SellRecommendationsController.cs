using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]"), Authorize]
    public class SellRecommendationsController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;

        public SellRecommendationsController(
            IUnitOfWorkFactory unitOfWork
            , UserManager<IdentityUser> userManager
            , IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.configuration = configuration;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            string userId = userManager.GetUserId(User);
            int pageSize = int.Parse(configuration["PaginationPageSize"]);

            var companies = unitOfWork.Company.GetAll();
            var lastPricies = await unitOfWork.Price.GetLastPricesAsync(30);
            var recommendations = lastPricies?
                .Join(unitOfWork.SellRecommendation.GetAll().Where(x => x.UserId.Equals(userId)), x => x.Key, y => y.CompanyId, (x, y) => new
                {
                    y.CompanyId,
                    LastPrice = x.Value,
                    y.PriceMin,
                    y.PriceMid,
                    y.PriceMax,
                    y.LotMin,
                    y.LotMid,
                    y.LotMax
                })
                .Where(x =>
                x.LastPrice >= x.PriceMin
                | x.LastPrice >= x.PriceMid
                | x.LastPrice >= x.PriceMax)
                .OrderBy(x => x.PriceMin / x.LastPrice)
                .ThenBy(x => x.PriceMid / x.LastPrice)
                .ThenBy(x => x.PriceMax / x.LastPrice);

            if (recommendations is null)
                return NoContent();

            var items = recommendations
                .Skip((value - 1) * pageSize)
                .Take(pageSize)
                .Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView
                {
                    Id = y.Id,
                    Name = y.Name,
                    Description = BuildDescription(x.LotMin, x.LotMid, x.LotMax, x.PriceMin, x.PriceMid, x.PriceMax)
                })
                .ToList();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Pagination.SetPagination(recommendations.Count(), value, pageSize);
            paginationResult.Items = items;

            return Ok(paginationResult);

            static string BuildDescription(int lmin, int lmid, int lmax, decimal pmin, decimal pmid, decimal pmax)
            {
                string result = "";
                if (lmin > 0)
                    result += $"{lmin}:{pmin:#,#0.##}";
                if (lmid > 0)
                    result += $" | {lmid}:{pmid:#,#0.##}";
                if (lmax > 0)
                    result += $" | {lmax}:{pmax:#,#0.##}";

                return result;
            }
        }

        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            string userId = userManager.GetUserId(User);
            var recommentation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == id);

            return recommentation is null ? NoContent() : Ok(new SellRecommendationModel
            {
                DateUpdate = recommentation.DateUpdate,
                LotMin = recommentation.LotMin,
                LotMid = recommentation.LotMid,
                LotMax = recommentation.LotMax,
                PriceMin = recommentation.PriceMin,
                PriceMid = recommentation.PriceMid,
                PriceMax = recommentation.PriceMax
            });
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            string userId = userManager.GetUserId(User);
            var recommentation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == id);

            return recommentation is null ? NoContent() : Ok(new SummarySellRecommendation
            {
                DateUpdate = recommentation.DateUpdate,
                LotMin = recommentation.LotMin,
                LotMid = recommentation.LotMid,
                LotMax = recommentation.LotMax,
                PriceMin = recommentation.PriceMin,
                PriceMid = recommentation.PriceMid,
                PriceMax = recommentation.PriceMax
            });
        }

        #region React
        [HttpGet("react/")]
        public async Task<SellRecommendationModel> Get(long companyId)
        {
            string userId = userManager.GetUserId(User);
            var recommentation = await unitOfWork.SellRecommendation.GetAll().FirstOrDefaultAsync(x => x.UserId.Equals(userId) && x.CompanyId == companyId);

            return recommentation is null ? null : new()
            {
                DateUpdate = recommentation.DateUpdate,
                LotMin = recommentation.LotMin,
                LotMid = recommentation.LotMid,
                LotMax = recommentation.LotMax,
                PriceMin = recommentation.PriceMin,
                PriceMid = recommentation.PriceMid,
                PriceMax = recommentation.PriceMax
            };
        }
        #endregion
    }
}
