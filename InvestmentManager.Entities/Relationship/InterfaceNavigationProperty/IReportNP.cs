using InvestmentManager.Entities.Market;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IReportNP
    {
        IEnumerable<Report> Reports { get; set; }
    }
}
