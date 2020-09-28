using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Market
{
    public class Industry : BaseEntity, ICompanyNP
    {
        [StringLength(300)]
        [Required]
        public string Name { get; set; }

        public virtual IEnumerable<Company> Companies { get; set; }
    }
}
