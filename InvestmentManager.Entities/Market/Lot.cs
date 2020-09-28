using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;

namespace InvestManager.Entities.Market
{
    public class Lot : BaseEntity, ITickerNP
    {
        public int Value { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}
