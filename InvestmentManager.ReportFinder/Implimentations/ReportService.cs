using InvestmentManager.Entities.Market;
using InvestmentManager.ReportFinder.Interfaces;
using InvestmentManager.Repository;
using InvestmentManager.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.ReportFinder.Implimentations
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
                ? await reportSources[sourceKey].GetNewReportsAsync(companyId, sourceValue)
                : resultReport;
        }
    }
}
