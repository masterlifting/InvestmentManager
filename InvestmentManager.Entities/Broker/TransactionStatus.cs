﻿using InvestmentManager.Entities.Basic;
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

        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<AccountTransaction> AccountTransactions { get; set; }
        public virtual IEnumerable<ExchangeRate> ExchangeRates { get; set; }
    }
}
