using InvestManager.Repository;
using InvestManager.ViewModels.CompanyModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace InvestManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public CompaniesController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;
        public IEnumerable<CompanyViewModel> Get()
        {
            var result = new List<CompanyViewModel>();
            foreach (var i in unitOfWork.Company.GetAll().Select(x => new { x.Id, x.Name }).OrderBy(x => x.Name))
            {
                result.Add(new CompanyViewModel
                {
                    Id = i.Id,
                    Name = i.Name
                });
            }

            return result;
        }
    }
}
