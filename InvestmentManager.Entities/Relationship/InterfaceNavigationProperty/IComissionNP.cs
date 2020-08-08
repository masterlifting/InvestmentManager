using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IComissionNP
    {
        List<Comission> Comissions { get; set; }
    }
}
