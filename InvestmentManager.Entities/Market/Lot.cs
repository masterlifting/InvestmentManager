using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Market
{
    public class Lot : BaseEntity, ITickerNP
    {
        public int Value { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}
