using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IReportMapper
    {
        Task<List<AccountTransaction>> MapToAccountTransactionsAsync(IEnumerable<BrockerAccountTransactionModel> models, long accountId);
        Task<List<StockTransaction>> MapToStockTransactionsAsync(IEnumerable<BrockerStockTransactionModel> models, long accountId);
        Task<List<Dividend>> MapToDividendsAsync(IEnumerable<BrockerDividendModel> models, long accountId);
        Task<List<Comission>> MapToComissionsAsync(IEnumerable<BrockerComissionModel> models, long accountId);
        Task<List<ExchangeRate>> MapToExchangeRatesAsync(IEnumerable<BrockerExchangeRateModel> models, long accountId);
    }
}
