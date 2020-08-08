using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Calculate
{
    public class BuyRecommendation : BaseEntity, ICompanyFK
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Price { get; set; }

        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
