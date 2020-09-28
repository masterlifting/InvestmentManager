using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Broker
{
    public class ComissionType : BaseEntity, IComissionNP
    {
        [StringLength(500)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Comission> Comissions { get; set; }
    }
}
