using InvestmentManager.Entities.Market;
using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IIsinNP
    {
        List<Isin> Isins { get; set; }
    }
}
