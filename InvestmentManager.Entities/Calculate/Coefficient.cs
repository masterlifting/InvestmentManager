using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Calculate
{
    public class Coefficient : BaseEntity
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal PE { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal PB { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal DebtLoad { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Profitability { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal ROA { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal ROE { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal EPS { get; set; }

        public long ReportId { get; set; }
        public virtual Report Report { get; set; }
    }
}
