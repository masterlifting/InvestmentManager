using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.ReportFinder.Interfaces
{
    internal interface IReportAgregator
    {
        Task<List<Report>> GetNewReportsAsync(long companyId, string sourceValue, object additional = null);
    }
}
