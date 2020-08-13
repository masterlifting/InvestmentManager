using InvestmentManager.BrokerService.Interfaces;

namespace InvestmentManager.BrokerService
{
    public interface IInvestBrokerService
    {
        IBcsParser BcsParser { get; }
        IReportMapper ReportMapper { get; }
        IReportFilter ReportFilter { get; }
    }
}
