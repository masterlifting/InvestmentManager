using InvestmentManager.Entities.Market;
using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IIsinNP
    {
        IEnumerable<Isin> Isins { get; set; }
    }
}
