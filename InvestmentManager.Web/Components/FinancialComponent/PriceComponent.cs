using InvestmentManager.Web.ViewAgregator.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace InvestmentManager.Web.Components.FinancialComponent
{
    public class PriceComponent : ViewComponent
    {
        private readonly IFinancialAgregator agregator;
        public PriceComponent(IFinancialAgregator agregator) => this.agregator = agregator;

        public async Task<IViewComponentResult> InvokeAsync(long id) => View(nameof(PriceComponent), await agregator.GetPricesComponentAsync(id).ConfigureAwait(false));
    }
}
