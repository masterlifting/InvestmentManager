using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Exchange : BaseEntity
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<Weekend> Weekend { get; set; }
    }
}
