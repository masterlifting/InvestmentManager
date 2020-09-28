using InvestmentManager.BrokerService.Models;
using System.Data;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IBcsParser
    {
        StringReportModel ParseBcsReport(DataSet excelReport);
        FilterReportModel ParsePeriodReport(DataSet excelReport);
    }
}
