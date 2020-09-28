using InvestManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IComissionNP
    {
        IEnumerable<Comission> Comissions { get; set; }
    }
}
