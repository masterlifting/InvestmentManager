using InvestmentManager.Entities.Broker;
using System.Threading.Tasks;
using static InvestmentManager.Models.Enums;

namespace InvestmentManager.Services.Interfaces
{
    public interface IReckonerService
    {
        Task<bool> UpgradeByPriceChangeAsync(DataBaseType dbType, string[] userIds);
        Task<bool> UpgradeByReportChangeAsync(DataBaseType dbType, long companyId, string[] userIds);
        Task<bool> UpgradeByStockTransactionChangeAsync(StockTransaction transaction, string userId);
        Task<bool> UpgradeByAccountTransactionChangeAsync(AccountTransaction transaction);
        Task<bool> UpgradeByDividendChangeAsync(Dividend dividend);
        Task<bool> UpgradeByComissionChangeAsync(Comission comission);
        Task<bool> UpgradeByExchangeRateChangeAsync(ExchangeRate exchangeRate);
    }
}
