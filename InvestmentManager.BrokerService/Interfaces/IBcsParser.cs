using InvestmentManager.BrokerService.Models;
using System.Data;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IBcsParser
    {
        BrokerReportModel ParseBcsReport(DataSet excelReport);
        FilterReportModel ParsePeriodReport(DataSet excelReport);
    }
}
