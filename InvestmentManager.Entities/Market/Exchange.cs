﻿using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Exchange : BaseEntity, ITickerNP, IStockTransactioNP
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        [StringLength(100)]
        [Required]
        public string ProviderName { get; set; }

        [StringLength(100)]
        [Required]
        public string ProviderUri { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
    }
}
