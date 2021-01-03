using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Broker;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class Isin : BaseEntity
    {
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        public long CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual IEnumerable<Dividend> Dividends { get; set; }
    }
}
