using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Web.Components.FinantialComponents
{
    public class ReportBodyComponent : ViewComponent
    {
        private readonly IFinancialAgregator agregator;
        public ReportBodyComponent(IFinancialAgregator agregator) => this.agregator = agregator;

        public IViewComponentResult Invoke(long? companyId) => View(nameof(ReportBodyComponent), agregator.BuildReportBody(companyId));
    }
}
