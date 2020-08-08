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
        public Company Company { get; set; }

        public long ExchangeId { get; set; }
        public Exchange Exchange { get; set; }

        public long LotId { get; set; }
        public Lot Lot { get; set; }

        public List<StockTransaction> StockTransactions { get; set; }
        public List<Price> Prices { get; set; }
    }
}
