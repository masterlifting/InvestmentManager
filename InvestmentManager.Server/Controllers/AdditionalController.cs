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
        public AdditionalController(IWebService webService) => this.webService = webService;

        [HttpGet("getrate")]
        public async Task<CBRF> GetDollar()
        {
            var rate = await webService.GetDollarRateAsync().ConfigureAwait(false);
            return new CBRF
            {
                Date = rate.Date,
                Valute = new Valute
                {
                    USD = new USD
                    {
                        Value = rate.Valute.USD.Value
                    }
                }
            };
        }
    }
}
