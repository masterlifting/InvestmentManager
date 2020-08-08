using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class AccountTransaction : BaseBroker, ITransactionStatusFK
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Amount { get; set; }

        public long TransactionStatusId { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
    }
}
