using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Broker
{
    public class TransactionStatus : BaseEntity, IStockTransactioNP, IAccountTransactionNP, IExchaneRateNP
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        public List<StockTransaction> StockTransactions { get; set; }
        public List<AccountTransaction> AccountTransactions { get; set; }
        public List<ExchangeRate> ExchangeRates { get; set; }
    }
}
