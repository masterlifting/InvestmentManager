using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class AccountSummary : BaseEntity
    {
        public long AccountId { get; set; }
        public virtual Account Account { get; set; }

        public long CurrencyId { get; set; }
        public virtual Currency Currency { get; set; }

        public decimal FreeSum { get; set; }
        public decimal InvestedSum { get; set; }
    }
}
