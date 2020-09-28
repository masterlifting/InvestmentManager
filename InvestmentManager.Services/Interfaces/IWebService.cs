using InvestManager.ViewModels.OutsideRequestModels;
using System.Net.Http;
using System.Threading.Tasks;

namespace InvestManager.Services.Interfaces
{
    public interface IWebService
    {
        Task<HttpResponseMessage> GetDataAsync(string query);
        Task<CBRF> GetDollarRateAsync();
    }
}
