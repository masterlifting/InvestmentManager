using InvestManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IDividendNP
    {
        IEnumerable<Dividend> Dividends { get; set; }
    }
}
