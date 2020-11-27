using InvestmentManager.Models;
using InvestmentManager.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
        public async Task<List<BaseView>> GetCurrencyTypes() =>
            await unitOfWork.Currency.GetAll().OrderBy(x => x.Name).Select(x => new BaseView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
        [HttpGet("exchangetypes/")]
        public async Task<List<BaseView>> GetExchangeTypes() =>
            await unitOfWork.Exchange.GetAll().OrderBy(x => x.Name).Select(x => new BaseView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
        [HttpGet("comissiontypes/")]
        public async Task<List<BaseView>> GetComissionTypes() =>
            await unitOfWork.ComissionType.GetAll().OrderBy(x => x.Name).Select(x => new BaseView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
        [HttpGet("lottypes/")]
        public async Task<List<BaseView>> GetLotTypes() =>
            await unitOfWork.Lot.GetAll().OrderBy(x => x.Value).Select(x => new BaseView { Id = x.Id, Name = x.Value.ToString() }).ToListAsync().ConfigureAwait(false);
        [HttpGet("statustypes/")]
        public async Task<List<BaseView>> GetStatusTypes() =>
            await unitOfWork.TransactionStatus.GetAll().OrderBy(x => x.Name).Select(x => new BaseView { Id = x.Id, Name = x.Name }).ToListAsync().ConfigureAwait(false);
    }
}
