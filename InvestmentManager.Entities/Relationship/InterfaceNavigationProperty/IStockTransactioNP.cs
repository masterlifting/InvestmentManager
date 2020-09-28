using InvestManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IStockTransactioNP
    {
        IEnumerable<StockTransaction> StockTransactions { get; set; }
    }
}
