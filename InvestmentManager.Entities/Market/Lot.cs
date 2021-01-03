using InvestmentManager.Entities.Basic;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Market
{
    public class Lot : BaseEntity
    {
        public int Value { get; set; }

        public virtual IEnumerable<Ticker> Tickers { get; set; }
    }
}
