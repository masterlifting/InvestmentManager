using InvestManager.Entities.Basic;
using InvestManager.Entities.Market;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Broker
{
    public class Dividend : BaseBroker, IIsinFK
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Tax { get; set; }

        public long IsinId { get; set; }
        public virtual Isin Isin { get; set; }
    }
}
