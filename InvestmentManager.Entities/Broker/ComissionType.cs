using InvestmentManager.Entities.Basic;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Broker
{
    public class ComissionType : BaseEntity
    {
        [StringLength(500)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Comission> Comissions { get; set; }
    }
}
