using InvestManager.Entities.Broker;

namespace InvestManager.Entities.Relationship.InterfaceForeignKey
{
    public interface ITransactionStatusFK
    {
        long TransactionStatusId { get; set; }
        TransactionStatus TransactionStatus { get; set; }
    }
}
