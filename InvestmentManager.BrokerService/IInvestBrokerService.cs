using InvestmentManager.BrokerService.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService
{
    public interface IInvestBrokerService
    {
        Task<ResultBrokerReportModel> GetNewReportsAsync(IFormFileCollection files, string userId);
    }
}
