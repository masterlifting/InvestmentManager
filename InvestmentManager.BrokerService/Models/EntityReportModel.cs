using InvestmentManager.Entities.Broker;
using System.Collections.Generic;

namespace InvestmentManager.BrokerService.Models
{
    public class EntityReportModel
    {
        public long AccountId { get; set; }

        public IEnumerable<ExchangeRate> ExchangeRates { get; set; } = new List<ExchangeRate>();
        public IEnumerable<Dividend> Dividends { get; set; } = new List<Dividend>();
        public IEnumerable<Comission> Comissions { get; set; } = new List<Comission>();
        public IEnumerable<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
        public IEnumerable<AccountTransaction> AccountTransactions { get; set; } = new List<AccountTransaction>();
    }
}
