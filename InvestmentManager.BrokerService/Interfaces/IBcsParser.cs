using InvestManager.BrokerService.Models;
using System.Data;

namespace InvestManager.BrokerService.Interfaces
{
    public interface IBcsParser
    {
        StringReportModel ParseBcsReport(DataSet excelReport);
        FilterReportModel ParsePeriodReport(DataSet excelReport);
    }
}
