using InvestManager.Entities.Market;
using InvestManager.ReportFinder.Interfaces;
using InvestManager.Repository;
using InvestManager.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestManager.ReportFinder.Implimentations
{
    public class ReportService : IReportService
    {

        private readonly Dictionary<string, IReportAgregator> reportSources;

        public ReportService(IWebService httpService, IUnitOfWorkFactory unitOfWork, IConverterService converterService)
        {
            // !!! Если появятся новые источники отчетов, то просто добавь их сюда и реализуй IReportAgregator
            reportSources = new Dictionary<string, IReportAgregator>
            {
                { "Investing", new InvestingAgregator(httpService, unitOfWork, converterService) }
            };
        }

        public async Task<List<Report>> FindNewReportsAsync(long companyId, string sourceKey, string sourceValue, object additional = null)
        {
            var resultReport = new List<Report>();

            return reportSources.ContainsKey(sourceKey)
                ? await reportSources[sourceKey].GetNewReportsAsync(companyId, sourceValue).ConfigureAwait(false)
                : resultReport;
        }
    }
}
