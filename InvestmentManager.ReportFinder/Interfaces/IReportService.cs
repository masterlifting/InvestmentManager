using InvestmentManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.ReportFinder.Interfaces
{
    public interface IReportService
    {
        Task<List<Report>> FindNewReportsAsync(long companyId, string sourceKey, string sourceValue, object additional = null);
    }
}
