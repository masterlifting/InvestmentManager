using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IndexController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public const string defaultResult = DefaultData.errorData;
        public IndexController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        public async Task<IndexModel> Get()
        {
            string companyCount = defaultResult;
            string companyDateUpdate = "";
            string reportCount = defaultResult;
            string reportDateUpdate = "";
            string coefficientCount = defaultResult;
            string coefficientDateUpdate = "";
            string priceCount = defaultResult;
            string priceDateUpdate = "";
            string buyRecommendationCount = defaultResult;
            string buyRecommendationDateUpdate = "";
            string ratingDateUpdate = defaultResult;

            var company = unitOfWork.Company.GetAll().OrderByDescending(x => x.DateUpdate).Select(x => new { x.Id, x.DateUpdate });
            var report = unitOfWork.Report.GetAll().OrderByDescending(x => x.DateUpdate).Select(x => new { x.Id, x.DateUpdate });
            var coefficient = unitOfWork.Coefficient.GetAll().OrderByDescending(x => x.DateUpdate).Select(x => new { x.Id, x.DateUpdate });
            var price = unitOfWork.Price.GetAll().OrderByDescending(x => x.DateUpdate).Select(x => new { x.Id, x.DateUpdate });
            var buyRecommendation = unitOfWork.BuyRecommendation.GetAll().Select(x => new { x.Id, x.DateUpdate });
            var rating = unitOfWork.Rating.GetAll().OrderByDescending(x => x.DateUpdate).Select(x => new { x.DateUpdate });

            if (company != null)
            {
                int count = await company.CountAsync().ConfigureAwait(false);
                DateTime date = (await company.FirstAsync().ConfigureAwait(false)).DateUpdate;
                companyCount = count.ToString();
                companyDateUpdate = date.ToString("g");
            }
            if (report != null)
            {
                int count = await report.CountAsync().ConfigureAwait(false);
                DateTime date = (await report.FirstAsync().ConfigureAwait(false)).DateUpdate;
                reportCount = count.ToString();
                reportDateUpdate = date.ToString("g");
            }
            if (coefficient != null)
            {
                int count = await coefficient.CountAsync().ConfigureAwait(false);
                DateTime date = (await coefficient.FirstAsync().ConfigureAwait(false)).DateUpdate;
                coefficientCount = count.ToString();
                coefficientDateUpdate = date.ToString("g");
            }
            if (price != null)
            {
                int count = await price.CountAsync().ConfigureAwait(false);
                DateTime date = (await price.FirstAsync().ConfigureAwait(false)).DateUpdate;
                priceCount = count.ToString();
                priceDateUpdate = date.ToString("g");
            }
            if (buyRecommendation != null)
            {
                int count = await buyRecommendation.CountAsync().ConfigureAwait(false);
                DateTime date = (await buyRecommendation.FirstAsync().ConfigureAwait(false)).DateUpdate;
                buyRecommendationCount = count.ToString();
                buyRecommendationDateUpdate = date.ToString("g");
            }
            if (rating != null)
            {
                DateTime date = (await rating.FirstAsync().ConfigureAwait(false)).DateUpdate;
                ratingDateUpdate = date.ToString("g");
            }

            return new IndexModel
            {
                CompanyCount = companyCount,
                CompanyDateUpdate = companyDateUpdate,
                ReportCount = reportCount,
                ReportDateUpdate = reportDateUpdate,
                CoefficientCount = coefficientCount,
                CoefficientDateUpdate = coefficientDateUpdate,
                PriceCount = priceCount,
                PriceDateUpdate = priceDateUpdate,
                BuyRecommendationCount = buyRecommendationCount,
                BuyRecommendationDateUpdate = buyRecommendationDateUpdate,
                RatingDateUpdate = ratingDateUpdate
            };
        }
    }
}
