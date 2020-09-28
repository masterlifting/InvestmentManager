using InvestmentManager.BrokerService.Models;
using InvestmentManager.Entities.Broker;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvestmentManager.BrokerService.Interfaces
{
    public interface IReportMapper
    {
        Task<List<AccountTransaction>> MapToAccountTransactionsAsync(IEnumerable<StringAccountTransactionModel> models, long accountId, List<ErrorReportModel> errors);
        Task<List<StockTransaction>> MapToStockTransactionsAsync(IEnumerable<StringStockTransactionModel> models, long accountId, List<ErrorReportModel> errors);
        Task<List<Dividend>> MapToDividendsAsync(IEnumerable<StringDividendModel> models, long accountId, List<ErrorReportModel> errors);
        Task<List<Comission>> MapToComissionsAsync(IEnumerable<StringComissionModel> models, long accountId, List<ErrorReportModel> errors);
        Task<List<ExchangeRate>> MapToExchangeRatesAsync(IEnumerable<StringExchangeRateModel> models, long accountId, List<ErrorReportModel> errors);
    }
}
