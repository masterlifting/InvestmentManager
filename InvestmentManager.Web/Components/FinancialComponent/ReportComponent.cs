using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Components.FinancialComponent
{
    public class ReportComponent : ViewComponent
    {
        private readonly IFinancialAgregator agregator;
        public ReportComponent(IFinancialAgregator agregator) => this.agregator = agregator;

        public async Task<IViewComponentResult> InvokeAsync(long id) => View(nameof(ReportComponent), await agregator.GetReportsComponentAsync(id).ConfigureAwait(false));
    }
}
