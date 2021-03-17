using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
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
    public class RatingsController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConfiguration configuration;

        public RatingsController(IUnitOfWorkFactory unitOfWork, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = int.Parse(configuration["PaginationPageSize"]);
            
            var companies = unitOfWork.Company.GetAll();
            var ratings = unitOfWork.Rating.GetAll().OrderByDescending(x => x.Result);

            var result = ratings?.Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView { Id = y.Id, Name = y.Name, Description = x.Result.ToString("f2") });

            if (result is null)
                return NoContent();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Items = await result.Skip((value - 1) * pageSize).Take(pageSize).ToListAsync();
            paginationResult.Pagination.SetPagination(await result.CountAsync(), value, pageSize);

            return Ok(paginationResult);
        }
        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var result = (await unitOfWork.Company.FindByIdAsync(id))?.Rating;

            return result is null ? NoContent() : Ok(new RatingModel
            {
                CashFlowPositiveBalanceValue = result.CashFlowPositiveBalanceValue,
                CoefficientAverageValue = result.CoefficientAverageValue,
                CoefficientComparisonValue = result.CoefficientComparisonValue,
                PriceComparisonValue = result.PriceComparisonValue,
                ReportComparisonValue = result.ReportComparisonValue
            });
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            var ratingCount = await unitOfWork.Rating.GetAll().CountAsync();
            var result = (await unitOfWork.Company.FindByIdAsync(id))?.Rating;

            return result is null ? NoContent() : Ok(new SummaryRating
            {
                DateUpdate = result.DateUpdate,
                PlaceCurrent = result.Place,
                ValueTotal = result.Result,
                PlaceTotal = ratingCount
            });
        }
    }
}
