using InvestManager.Entities.Basic;
using InvestManager.Entities.Broker;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using InvestManager.Entities.Relationship.InterfaceNavigationProperty;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Market
{
    public class Isin : BaseEntity, ICompanyFK, IDividendNP
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual IEnumerable<Dividend> Dividends { get; set; }
    }
}
