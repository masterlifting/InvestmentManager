using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Ticker : BaseEntity, ICompanyFK, IExchangeFK, ILotFK, IStockTransactioNP, IPriceNP
    {
        [StringLength(10)]
        [Required]
        public string Name { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public long ExchangeId { get; set; }
        public virtual Exchange Exchange { get; set; }

        public long LotId { get; set; }
        public virtual Lot Lot { get; set; }

        public virtual IEnumerable<StockTransaction> StockTransactions { get; set; }
        public virtual IEnumerable<Price> Prices { get; set; }
    }
}
