using InvestmentManager.Web.Models.FinancialModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Web.ViewAgregator.Interfaces
{
    public interface IFinancialAgregator
    {
        Task<List<PriceModel>> GetPricesAsync(long? companyId);
        Task<ReportModel> GetReportsAsync(long? companyId);
        List<ReportBodyModel> BuildReportBody(long? companyId);
    }
}
