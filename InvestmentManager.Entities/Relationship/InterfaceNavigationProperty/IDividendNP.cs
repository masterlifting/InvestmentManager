using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IDividendNP
    {
        List<Dividend> Dividends { get; set; }
    }
}
