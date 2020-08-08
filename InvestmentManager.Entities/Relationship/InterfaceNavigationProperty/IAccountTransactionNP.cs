using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IAccountTransactionNP
    {
        List<AccountTransaction> AccountTransactions { get; set; }
    }
}
