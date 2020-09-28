using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceForeignKey;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Market
{
    public class Price : BaseEntity, ICurrencyFK, ITickerFK
    {
        public DateTime BidDate { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Value { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public long TickerId { get; set; }
        public virtual Ticker Ticker { get; set; }
    }
}
