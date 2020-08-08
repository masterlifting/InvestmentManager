using InvestmentManager.Entities.Broker;

namespace InvestmentManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ITransactionStatusFK
    {
        long TransactionStatusId { get; set; }
        TransactionStatus TransactionStatus { get; set; }
    }
}
