using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Broker
{
    public class Account : BaseEntity, IDividendNP, IAccountTransactionNP, IStockTransactioNP, IComissionNP, IExchaneRateNP
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string UserId { get; set; }

        public List<Dividend> Dividends { get; set; }
        public List<AccountTransaction> AccountTransactions { get; set; }
        public List<StockTransaction> StockTransactions { get; set; }
        public List<Comission> Comissions { get; set; }
        public List<ExchangeRate> ExchangeRates { get; set; }
    }
}
