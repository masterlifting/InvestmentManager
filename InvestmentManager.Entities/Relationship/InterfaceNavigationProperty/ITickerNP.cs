using InvestManager.Entities.Market;
using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface ITickerNP
    {
        IEnumerable<Ticker> Tickers { get; set; }
    }
}
