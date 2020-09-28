using InvestmentManager.ViewModels.OutsideRequestModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Services.Interfaces
{
    public interface IWebService
    {
        Task<HttpResponseMessage> GetDataAsync(string query);
        Task<CBRF> GetDollarRateAsync();
    }
}
