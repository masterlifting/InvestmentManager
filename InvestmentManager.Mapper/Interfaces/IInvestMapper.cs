using InvestmentManager.BrokerService.Models;
using InvestmentManager.ViewModels;
using InvestmentManager.ViewModels.ReportModels.BrokerReportModels;

namespace InvestmentManager.Mapper.Interfaces
{
    public interface IInvestMapper
    {
        BrokerReportModel MapBcsReports(ResultBrokerReportModel resultReportsModell);
        CBRF MapCBRF(Services.Implimentations.CBRF currentModel);
    }
}
