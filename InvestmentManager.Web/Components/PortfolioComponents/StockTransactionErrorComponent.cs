using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Web.Components.PortfolioComponents
{
    public class StockTransactionErrorComponent : ViewComponent
    {
        private readonly IPortfolioAgregator agregator;
        public StockTransactionErrorComponent(IPortfolioAgregator agregator) => this.agregator = agregator;
        public IViewComponentResult Invoke() => View(nameof(StockTransactionErrorComponent), agregator.LoadStockTransactionErrorForm());
    }
}
