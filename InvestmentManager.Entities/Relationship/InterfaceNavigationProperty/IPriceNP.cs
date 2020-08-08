using InvestmentManager.Entities.Market;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IPriceNP
    {
        List<Price> Prices { get; set; }
    }
}
