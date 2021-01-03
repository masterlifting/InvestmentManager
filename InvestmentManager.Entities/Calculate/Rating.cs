using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;

namespace InvestmentManager.Entities.Calculate
{
    public class Rating : BaseEntity
    {
        public int Place { get; set; }
        public decimal Result { get; set; }

        public decimal? PriceComparisonValue { get; set; }
        public decimal? ReportComparisonValue { get; set; }
        public decimal? CashFlowPositiveBalanceValue { get; set; }
        public decimal? CoefficientComparisonValue { get; set; }
        public decimal? CoefficientAverageValue { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
