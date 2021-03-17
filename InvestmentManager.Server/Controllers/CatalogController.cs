using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        public CatalogController(IUnitOfWorkFactory unitOfWork) => this.unitOfWork = unitOfWork;

        [HttpGet("currencytypes/")]
        public async Task<IActionResult> GetCurrencyTypes() =>
            Ok(await unitOfWork.Currency.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());
        [HttpGet("exchangetypes/")]
        public async Task<IActionResult> GetExchangeTypes() =>
            Ok(await unitOfWork.Exchange.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());
        [HttpGet("comissiontypes/")]
        public async Task<IActionResult> GetComissionTypes() =>
            Ok(await unitOfWork.ComissionType.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());
        [HttpGet("lottypes/")]
        public async Task<IActionResult> GetLotTypes() =>
           Ok(await unitOfWork.Lot.GetAll().OrderBy(x => x.Value).Select(x => new ShortView { Id = x.Id, Name = x.Value.ToString() }).ToListAsync());
        [HttpGet("statustypes/")]
        public async Task<IActionResult> GetStatusTypes() =>
            Ok(await unitOfWork.TransactionStatus.GetAll().OrderBy(x => x.Name).Select(x => new ShortView { Id = x.Id, Name = x.Name }).ToListAsync());
    }
}
