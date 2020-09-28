using InvestmentManager.BrokerService.Models;
using InvestmentManager.ViewModels.PortfolioModels;

namespace InvestmentManager.Mapper.Interfaces
{
    public interface IPortfolioMapper
    {
        BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModell);
    }
}
