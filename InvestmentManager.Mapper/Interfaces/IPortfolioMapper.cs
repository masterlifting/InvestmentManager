using InvestmentManager.BrokerService.Models;
using InvestmentManager.ViewModels.ReportModels.BrokerReportModels;

namespace InvestmentManager.Mapper.Interfaces
{
    public interface IPortfolioMapper
    {
        BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModell);
    }
}
