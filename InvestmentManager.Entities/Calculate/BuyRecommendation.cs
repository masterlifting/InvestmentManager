using InvestManager.Entities.Basic;
using InvestManager.Entities.Market;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Calculate
{
    public class BuyRecommendation : BaseEntity, ICompanyFK
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Price { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }
    }
}
