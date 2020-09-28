using InvestManager.Entities.Market;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IPriceNP
    {
        IEnumerable<Price> Prices { get; set; }
    }
}
