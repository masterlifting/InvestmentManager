using InvestmentManager.Entities.Basic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class ExchangeRate : BaseBroker
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Rate { get; set; }

        public long TransactionStatusId { get; set; }
        public virtual TransactionStatus TransactionStatus { get; set; }
    }
}
