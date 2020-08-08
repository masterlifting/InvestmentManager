using InvestmentManager.BrokerService.Interfaces;

namespace InvestmentManager.BrokerService
{
    public interface ICustomBrokerService
    {
        IBcsParser BcsParser { get; }
        IReportMapper ReportMapper { get; }
        IReportFilter ReportFilter { get; }
    }
}
