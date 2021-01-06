using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class CompanySummary : BaseEntity
    {
        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public long AccountId { get; set; }
        public virtual Account Account { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public int ActualLot { get; set; }
        public decimal CurrentProfit { get; set; }
    }
}
