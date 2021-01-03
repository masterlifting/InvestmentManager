using InvestmentManager.Entities.Basic;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Market
{
    public class Price : BaseEntity
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
