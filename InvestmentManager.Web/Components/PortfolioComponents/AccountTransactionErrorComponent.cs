using InvestmentManager.Web.Models.PortfolioModels;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentManager.Web.Components.PortfolioComponents
{
    public class AccountTransactionErrorComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View(nameof(AccountTransactionErrorComponent), new AccountTransactionErrorForm());
    }
}
