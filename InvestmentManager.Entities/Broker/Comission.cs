using InvestmentManager.Entities.Basic;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class Comission : BaseBroker
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Amount { get; set; }

        public long ComissionTypeId { get; set; }
        public virtual ComissionType ComissionType { get; set; }
    }
}
