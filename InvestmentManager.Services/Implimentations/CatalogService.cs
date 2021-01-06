using InvestmentManager.Services.Interfaces;

namespace InvestmentManager.Services.Implimentations
{
    public class CatalogService : ICatalogService
    {
        public string GetStatusBootstrapColor(long statusId) => statusId switch
        {
            1 => "danger",
            2 => "success",
            3 => "danger",
            4 => "success",
            _ => "dark"
        };

        public string GetStatusName(long statusId) => statusId switch
        {
            1 => "Add",
            2 => "Withdraw",
            3 => "Buy",
            4 => "Sell",
            _ => "Status not found"
        };
        public string GetExchangeName(long exchangeId) => exchangeId switch
        {
            1 => "MMVB",
            2 => "SPB",
            _ => "Exchange not found"
        };
        public string GetCurrencyName(long currencyId) => currencyId switch
        {
            1 => "usd",
            2 => "rub",
            _ => "Currency not found"
        };
    }
}
