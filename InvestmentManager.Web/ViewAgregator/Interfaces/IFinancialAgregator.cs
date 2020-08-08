using InvestmentManager.Web.Models.FinancialModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.Web.ViewAgregator.Interfaces
{
    public interface IFinancialAgregator
    {
        Task<List<PriceComponentModel>> GetPricesComponentAsync(long id);
        Task<List<ReportComponentModel>> GetReportsComponentAsync(long id);
    }
}
