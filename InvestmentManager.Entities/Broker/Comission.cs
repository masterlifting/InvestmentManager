using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestManager.Entities.Broker
{
    public class Comission : BaseBroker, IComissionTypeFK
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Amount { get; set; }

        public long ComissionTypeId { get; set; }
        public virtual ComissionType ComissionType { get; set; }
    }
}
