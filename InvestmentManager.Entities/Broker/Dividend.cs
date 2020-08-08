using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Market;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class Dividend : BaseBroker, IIsinFK
    {
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal Tax { get; set; }

        public long IsinId { get; set; }
        public Isin Isin { get; set; }
    }
}
