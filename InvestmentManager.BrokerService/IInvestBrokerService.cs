using InvestManager.BrokerService.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace InvestManager.BrokerService
{
    public interface IInvestBrokerService
    {
        Task<ResultBrokerReportModel> GetNewReportsAsync(IFormFileCollection files, string userId);
    }
}
