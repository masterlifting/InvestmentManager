using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Broker
{
    public class ExchangeRate : BaseBroker, ITransactionStatusFK
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Rate { get; set; }

        public long TransactionStatusId { get; set; }
        public virtual TransactionStatus TransactionStatus { get; set; }
    }
}
