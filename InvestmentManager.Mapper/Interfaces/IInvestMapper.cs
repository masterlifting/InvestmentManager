using InvestmentManager.BrokerService.Models;
using InvestmentManager.Models.Services;

namespace InvestmentManager.Mapper.Interfaces
{
    public interface IInvestMapper
    {
        BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModell);
    }
}
