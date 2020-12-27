using InvestmentManager.Entities.Broker;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Interfaces
{
    public interface IReckonerService
    {
        Task UpgradeByPriceChangeAsync(DataBaseType dbType, string[] userIds);
        Task UpgradeByReportChangeAsync(DataBaseType dbType, long companyId, string[] userIds);
        Task UpgradeByStockTransactionChangeAsync(StockTransaction transaction, string userId);
        Task UpgradeByAccountTransactionChangeAsync(AccountTransaction transaction);
        Task UpgradeByDividendChangeAsync(Dividend dividend);
        Task UpgradeByComissionChangeAsync(Comission comission);
        Task UpgradeByExchangeRateChangeAsync(ExchangeRate exchangeRate);
    }
}
