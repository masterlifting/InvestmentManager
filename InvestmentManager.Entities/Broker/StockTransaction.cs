﻿using InvestManager.Entities.Basic;
using InvestManager.Entities.Market;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Broker
{
    public class StockTransaction : BaseBroker, ITransactionStatusFK, IExchangeFK
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
