using InvestManager.BrokerService.Models;
using InvestManager.ViewModels.PortfolioModels;

namespace InvestManager.Mapper.Interfaces
{
    public interface IPortfolioMapper
    {
        BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModell);
    }
}
