using InvestmentManager.BrokerService.Implimentations;
using InvestmentManager.BrokerService.Interfaces;
using InvestmentManager.Repository;

namespace InvestmentManager.BrokerService
{
    public class CustomBrokerService : ICustomBrokerService
    {
        private readonly IUnitOfWorkFactory unitOfWork;
        private readonly InvestmentContext context;

        public CustomBrokerService(IUnitOfWorkFactory unitOfWork, InvestmentContext context)
        {
            this.unitOfWork = unitOfWork;
            this.context = context;
        }

        public IBcsParser BcsParser => new BcsParser();
        public IReportMapper ReportMapper => new ReportMapper(unitOfWork);
        public IReportFilter ReportFilter => new ReportFilter(context);
    }
}
