using InvestManager.Entities.Market;
using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IIsinNP
    {
        IEnumerable<Isin> Isins { get; set; }
    }
}
