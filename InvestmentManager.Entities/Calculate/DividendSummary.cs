using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class DividendSummary : BaseEntity
    {
        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public long AccountId { get; set; }
        public virtual Account Account { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public decimal TotalSum { get; set; }
        public decimal TotalTax { get; set; }
    }
}
