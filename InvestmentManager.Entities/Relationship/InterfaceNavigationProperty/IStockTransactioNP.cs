using InvestmentManager.Entities.Broker;

using System.Collections.Generic;

namespace InvestmentManager.Entities.Relationship.InterfaceNavigationProperty
{
    public interface IStockTransactioNP
    {
        List<StockTransaction> StockTransactions { get; set; }
    }
}
