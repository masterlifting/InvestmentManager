using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class ExchangeRate : BaseBroker, ITransactionStatusFK
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Rate { get; set; }

        public long TransactionStatusId { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
    }
}
