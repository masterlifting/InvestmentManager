namespace InvestmentManager.Services.Interfaces
{
    public interface ICatalogService
    {
        string GetStatusName(long statusId);
        string GetStatusBootstrapColor(long statusId);
        string GetExchangeName(long exchangeId);
        string GetCurrencyName(long currencyId);
    }
}
