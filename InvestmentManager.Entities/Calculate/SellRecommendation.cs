using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Calculate
{
    public class SellRecommendation : BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string UserId { get; set; }

        public int LotMin { get; set; }
        public decimal PriceMin { get; set; }
        public int LotMid { get; set; }
        public decimal PriceMid { get; set; }
        public int LotMax { get; set; }
        public decimal PriceMax { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
