using InvestManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IExchaneRateNP
    {
        IEnumerable<ExchangeRate> ExchangeRates { get; set; }
    }
}
