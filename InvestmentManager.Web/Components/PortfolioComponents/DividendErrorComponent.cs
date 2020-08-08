using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Web.Components.PortfolioComponents
{
    public class DividendErrorComponent : ViewComponent
    {
        private readonly IPortfolioAgregator agregator;
        public DividendErrorComponent(IPortfolioAgregator agregator) => this.agregator = agregator;
        public IViewComponentResult Invoke() => View(nameof(DividendErrorComponent), agregator.LoadDividendErrorForm());
    }
}
