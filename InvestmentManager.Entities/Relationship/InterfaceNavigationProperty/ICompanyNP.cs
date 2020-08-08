using InvestmentManager.Entities.Market;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface ICompanyNP
    {
        List<Company> Companies { get; set; }
    }
}
