using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IDividendNP
    {
        IEnumerable<Dividend> Dividends { get; set; }
    }
}
