using InvestmentManager.Entities.Basic;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Sector : BaseEntity
    {
        [StringLength(300)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Company> Companies { get; set; }
    }
}
