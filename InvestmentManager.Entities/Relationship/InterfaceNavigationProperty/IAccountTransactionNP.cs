using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IAccountTransactionNP
    {
        IEnumerable<AccountTransaction> AccountTransactions { get; set; }
    }
}
