using InvestmentManager.Entities.Calculate;
using InvestmentManager.Models.EntityModels;
using InvestmentManager.Models.SummaryModels;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CoefficientsController : Controller
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly IConverterService converterService;

        public CoefficientsController(IUnitOfWorkFactory unitOfWork, IConverterService converterService)
        {
            this.unitOfWork = unitOfWork;
            this.converterService = converterService;
        }

        [HttpGet("bycompanyid/{id}")]
        public async Task<IActionResult> GetByCompanyId(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id);
            var reports = company.Reports.Where(x => x.IsChecked);
            var result = reports.Select(x => x.Coefficient);
            return !result.Any() ? NoContent() : Ok(result.Select(x => new CoefficientModel
            {
                PE = x.PE,
                PB = x.PB,
                EPS = x.EPS,
                Profitability = x.Profitability,
                ROA = x.ROA,
                ROE = x.ROE,
                DebtLoad = x.DebtLoad,
                Year = x.Report.DateReport.Year,
                Quarter = converterService.ConvertToQuarter(x.Report.DateReport.Month)
            }).ToList());
        }
        [HttpGet("bycompanyid/{id}/summary/")]
        public async Task<IActionResult> GetSummaryByCompanyId(long id)
        {
            var company = await unitOfWork.Company.FindByIdAsync(id);
            var reports = company.Reports.Where(x => x.IsChecked);
            var results = reports.Select(x => x.Coefficient).OrderBy(x => x.DateUpdate);
            return !results.Any() ? NoContent() : Ok(new SummaryCoefficient
            {
                DateUpdate = results.Last().DateUpdate,
                Count = results.Count(),
                Multiplcators = string.Join("; ", results.First().GetType().GetProperties()
                .Where(x => !x.Name.Equals("LazyLoader")
                                && !x.Name.Equals(nameof(Coefficient.DateUpdate))
                                && !x.Name.Equals(nameof(Coefficient.Id))
                                && !x.Name.Equals(nameof(Coefficient.Report))
                                && !x.Name.Equals(nameof(Coefficient.ReportId)))
                .Select(x => x.Name))
            });
        }
    }
}
