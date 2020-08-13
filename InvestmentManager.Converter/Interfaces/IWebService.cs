using System.Net.Http;
using System.Threading.Tasks;

namespace InvestmentManager.Service.Interfaces
{
    public interface IWebService
    {
        Task<HttpResponseMessage> GetDataAsync(string query);
    }
}
