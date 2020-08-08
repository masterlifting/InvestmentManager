using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class StockTransaction : BaseBroker, ITransactionStatusFK, IExchangeFK
    {
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Cost { get; set; }

        public long TickerId { get; set; }
        public Ticker Ticker { get; set; }

        public long TransactionStatusId { get; set; }
        public TransactionStatus TransactionStatus { get; set; }

        public long ExchangeId { get; set; }
        public Exchange Exchange { get; set; }
    }
}
