using InvestmentManager.ClientModels;
using InvestmentManager.Models.Additional;
using InvestmentManager.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace InvestmentManager.Server.Controllers
{
    [ApiController, Route("[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly IWebService webService;

        public ServiceController(IWebService webService)
        {
            this.webService = webService;
        }
        [HttpGet("rate/")]
        public async Task<ClientBaseResponse<CBRF>> GetRate()
        {

            var result = new ClientBaseResponse<CBRF>();
            var rate = await webService.GetCBRateAsync();


            if (!rate.IsSuccessStatusCode)
            {
                result.Errors = new[] { "request rate error" };
                return result;
            }

            result.IsSuccess = true;
            result.Data = await rate.Content.ReadFromJsonAsync<CBRF>();

            return result;
        }
    }
}
