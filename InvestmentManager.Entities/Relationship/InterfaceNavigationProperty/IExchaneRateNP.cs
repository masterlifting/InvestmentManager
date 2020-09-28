using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IExchaneRateNP
    {
        IEnumerable<ExchangeRate> ExchangeRates { get; set; }
    }
}
