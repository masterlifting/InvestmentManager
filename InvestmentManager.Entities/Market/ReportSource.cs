using InvestmentManager.Entities.Basic;
using InvestmentManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Entities.Market
{
    public class ReportSource : BaseEntity, ICompanyFK
    {
        [StringLength(100)]
        [Required]
        public string Key { get; set; }
        [StringLength(500)]
        [Required]
        public string Value { get; set; }

        public long CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
