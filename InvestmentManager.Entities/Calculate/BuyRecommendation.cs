using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class BuyRecommendation : BaseEntity
    {
        public decimal Price { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
