using InvestManager.Entities.Basic;
using InvestManager.Entities.Market;
using InvestManager.Entities.Relationship.InterfaceForeignKey;

using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Calculate
{
    public class Coefficient : BaseEntity, IReportFK
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
