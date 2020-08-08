using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvestmentManager.Entities.Broker
{
    public class Comission : BaseBroker, IComissionTypeFK
    {
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Amount { get; set; }

        public long ComissionTypeId { get; set; }
        public ComissionType ComissionType { get; set; }
    }
}
