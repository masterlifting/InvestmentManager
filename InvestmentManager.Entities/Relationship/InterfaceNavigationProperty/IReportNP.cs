using InvestmentManager.Entities.Market;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IReportNP
    {
        List<Report> Reports { get; set; }
    }
}
