using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Models.EntityModels
{
    public class PriceModel
    {
        public DateTime BidDate { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public DateTime DateUpdate { get; set; }
        public decimal Value { get; set; }
        public long CurrencyId { get; set; }
        public long TickerId { get; set; }
    }
}
