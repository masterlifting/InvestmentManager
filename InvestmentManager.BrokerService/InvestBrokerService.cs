using InvestmentManager.BrokerService.Interfaces;

namespace InvestmentManager.BrokerService
{
    public class InvestBrokerService : IInvestBrokerService
    {
        public InvestBrokerService(
        IBcsParser bcsParser
        , IReportMapper reportMapper
        , IReportFilter reportFilter)
        {
            BcsParser = bcsParser;
            ReportMapper = reportMapper;
            ReportFilter = reportFilter;
        }

        public IBcsParser BcsParser { get; }
        public IReportMapper ReportMapper { get; }
        public IReportFilter ReportFilter { get; }
    }
}
