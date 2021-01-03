using InvestmentManager.Entities.Basic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class AccountTransaction : BaseBroker
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Amount { get; set; }

        public long TransactionStatusId { get; set; }
        public virtual TransactionStatus TransactionStatus { get; set; }
    }
}
