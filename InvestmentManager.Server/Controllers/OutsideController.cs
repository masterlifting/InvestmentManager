using System.Threading.Tasks;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels.OutsideRequestModels;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OutsideController : ControllerBase
    {
        private readonly IWebService webService;
        public OutsideController(IWebService webService) => this.webService = webService;

        [Route("getrate")]
        public async Task<CBRF> Get() => await webService.GetDollarRateAsync().ConfigureAwait(false);
    }
}
