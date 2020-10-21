using InvestmentManager.Mapper.Interfaces;
using InvestmentManager.Services.Interfaces;
using InvestmentManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class AdditionalController : ControllerBase
    {
        private readonly IWebService webService;
        private readonly IInvestMapper mapper;

        public AdditionalController(
            IWebService webService
            , IInvestMapper mapper)
        {
            this.webService = webService;
            this.mapper = mapper;
        }

        [HttpGet("getrate")]
        public async Task<CBRF> GetDollar() => mapper.MapCBRF(await webService.GetDollarRateAsync().ConfigureAwait(false));
    }
}
