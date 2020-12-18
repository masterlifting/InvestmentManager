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
            1 => "USD",
            2 => "RUB",
            _ => "Currency not found"
        };

        public string GetComissionTypeName(long typeId) => typeId switch
        {
            1 => "НДФЛ",
            2 => "Урегулирование сделок",
            3 => "Вознаграждение компании",
            4 => "Комиссия за займы Овернайт ЦБ",
            5 => "Вознаграждение компании (СВОП)",
            6 => "Вознаграждение за обслуживание счета депо",
            7 => "Оплата за вывод денежных средств",
            8 => "Вознаграждение компании (репо)",
            9 => "Комиссия Биржевой гуру",
            _ => "Comission type not found"
        };
    }
}
