using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceNavigationProperty;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Industry : BaseEntity, ICompanyNP
    {
        [StringLength(300)]
        [Required]
        public string Name { get; set; }

        public List<Company> Companies { get; set; }
    }
}
