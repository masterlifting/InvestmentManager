using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Web.Components.PortfolioComponents
{
    public class CorrectErrorBrokerReportsComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View(nameof(CorrectErrorBrokerReportsComponent));
    }
}
