using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Currency : BaseEntity, IPriceNP, IDividendNP, IAccountTransactionNP, IStockTransactioNP, IComissionNP, IExchaneRateNP
    {
        [StringLength(10)]
        [Required]
        public string Name { get; set; }

        public List<Price> Prices { get; set; }
        public List<Dividend> Dividends { get; set; }
        public List<AccountTransaction> AccountTransactions { get; set; }
        public List<StockTransaction> StockTransactions { get; set; }
        public List<Comission> Comissions { get; set; }
        public List<ExchangeRate> ExchangeRates { get; set; }
    }
}
