using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;

using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Calculate
{
    public class Rating : BaseEntity, ICompanyFK
    {
        public int Place { get; set; }

        [Column(TypeName = "Decimal(18,5)")]
        public decimal Result { get; set; }


        [Column(TypeName = "Decimal(18,5)")]
        public decimal? PriceComparisonValue { get; set; }
        [Column(TypeName = "Decimal(18,5)")]
        public decimal? ReportComparisonValue { get; set; }
        [Column(TypeName = "Decimal(18,5)")]
        public decimal? CashFlowPositiveBalanceValue { get; set; }
        [Column(TypeName = "Decimal(18,5)")]
        public decimal? CoefficientComparisonValue { get; set; }
        [Column(TypeName = "Decimal(18,5)")]
        public decimal? CoefficientAverageValue { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
