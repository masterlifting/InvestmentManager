using InvestManager.Entities.Market;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestManager.ReportFinder.Interfaces
{
    internal interface IReportAgregator
    {
        Task<List<Report>> GetNewReportsAsync(long companyId, string sourceValue, object additional = null);
    }
}
