﻿using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Broker
{
    public class Account : BaseEntity, IDividendNP, IAccountTransactionNP, IStockTransactioNP, IComissionNP, IExchaneRateNP
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string UserId { get; set; }

        public virtual IEnumerable<Dividend> Dividends { get; set; }
        public virtual IEnumerable<AccountTransaction> AccountTransactions { get; set; }
        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<Comission> Comissions { get; set; }
        public virtual IEnumerable<ExchangeRate> ExchangeRates { get; set; }
    }
}
