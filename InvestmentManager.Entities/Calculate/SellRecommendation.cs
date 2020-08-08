using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Calculate
{
    public class SellRecommendation : BaseEntity, ICompanyFK
    {
        [Required]
        [StringLength(100)]
        public string UserId { get; set; }

        [Column(TypeName = "Decimal(18,2)")]
        public int LotMin { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal PriceMin { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public int LotMid { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal PriceMid { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public int LotMax { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal PriceMax { get; set; }

        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
