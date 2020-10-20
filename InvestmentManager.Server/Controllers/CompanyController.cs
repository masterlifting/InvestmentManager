using InvestmentManager.Repository;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public CompanyController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("additional")]
        public async Task<CompanyAdditionalInfoShortModel> GetAdditional(long id)
        {
            var result = await unitOfWork.Company.FindByIdAsync(id).ConfigureAwait(false);
            return new CompanyAdditionalInfoShortModel
            {
                Industry = result.Industry.Name,
                Sector = result.Sector.Name,
                Currency = result.Tickers.FirstOrDefault().Prices.FirstOrDefault().Currency.Name
            };
        }
        [HttpGet("list")]
        public async Task<List<ViewModelBase>> GetCompanyList()
        {
            return await unitOfWork.Company.GetAll()
                .OrderBy(x => x.Name)
                .Select(x => new ViewModelBase { Id = x.Id, Name = x.Name })
                .ToListAsync().ConfigureAwait(false);
        }
    }
}
