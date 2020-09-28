using InvestManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IAccountTransactionNP
    {
        IEnumerable<AccountTransaction> AccountTransactions { get; set; }
    }
}
