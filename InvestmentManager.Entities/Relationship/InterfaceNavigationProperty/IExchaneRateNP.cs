using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IExchaneRateNP
    {
        List<ExchangeRate> ExchangeRates { get; set; }
    }
}
