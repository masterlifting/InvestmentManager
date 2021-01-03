using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class StockTransaction : BaseBroker
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Cost { get; set; }

        public long TickerId { get; set; }
        public virtual Ticker Ticker { get; set; }

        public long TransactionStatusId { get; set; }
        public virtual TransactionStatus TransactionStatus { get; set; }

        public long ExchangeId { get; set; }
        public virtual Exchange Exchange { get; set; }
    }
}
