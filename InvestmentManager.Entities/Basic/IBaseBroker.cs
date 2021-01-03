using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;
using System;

namespace InvestmentManager.Entities.Basic
{
    public interface IBaseBroker : IBaseEntity
    {
        DateTime DateOperation { get; set; }

        long AccountId { get; set; }
        Account Account { get; set; }

        long CurrencyId { get; set; }
        Currency Currency { get; set; }
    }
}
