using InvestmentManager.Entities.Market;
using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface ITickerNP
    {
        IEnumerable<Ticker> Tickers { get; set; }
    }
}
