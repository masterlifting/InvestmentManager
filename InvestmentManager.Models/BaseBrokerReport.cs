using System;

namespace InvestmentManager.Models
{
    public abstract class BaseBrokerReport
    {
        public long CurrencyId { get; set; }
        public long AccountId { get; set; }
        public DateTime DateOperation { get; set; }
    }
}
