using InvestmentManager.Models;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class RatingsController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public RatingsController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("bypagination/{value}")]
        public async Task<IActionResult> GetPagination(int value = 1)
        {
            int pageSize = 10;
            var companies = unitOfWork.Company.GetAll();
            var ratings = unitOfWork.Rating.GetAll().OrderByDescending(x => x.Result);

            var result = ratings?.Join(companies, x => x.CompanyId, y => y.Id, (x, y) => new ShortView { Id = y.Id, Name = y.Name });

            if (result is null)
                return NoContent();

            var paginationResult = new PaginationViewModel<ShortView>();
            paginationResult.Items = await result.Skip((value - 1) * pageSize).Take(pageSize).ToListAsync().ConfigureAwait(false);
            paginationResult.Pagination.SetPagination(await result.CountAsync().ConfigureAwait(false), value, pageSize);

            return Ok(paginationResult);
        }
        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var result = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Rating;

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
            var ratingCount = await unitOfWork.Rating.GetAll().CountAsync().ConfigureAwait(false);
            var result = (await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false))?.Rating;

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
