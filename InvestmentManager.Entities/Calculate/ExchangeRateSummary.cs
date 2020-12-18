using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class ExchangeRateSummary : BaseEntity
    {
        public long AccountId { get; set; }
        public virtual Account Account { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public decimal TotalSoldQuantity { get; set; }
        public decimal TotalSoldCost { get; set; }
        public decimal TotalPurchasedQuantity { get; set; }
        public decimal TotalPurchasedCost { get; set; }
        public decimal AvgSoldRate { get; set; }
        public decimal AvgPurchasedRate { get; set; }
    }
}
