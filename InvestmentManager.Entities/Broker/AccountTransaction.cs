using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Broker
{
    public class AccountTransaction : BaseBroker, ITransactionStatusFK
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Amount { get; set; }

        public long TransactionStatusId { get; set; }
        public virtual TransactionStatus TransactionStatus { get; set; }
    }
}
