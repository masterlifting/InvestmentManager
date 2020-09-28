using InvestManager.Entities.Market;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface ICompanyNP
    {
        IEnumerable<Company> Companies { get; set; }
    }
}
