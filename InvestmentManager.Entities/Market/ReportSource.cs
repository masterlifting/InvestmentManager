using InvestManager.Entities.Basic;
using InvestManager.Entities.Relationship.InterfaceForeignKey;
using System.ComponentModel.DataAnnotations;

namespace InvestManager.Entities.Market
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
        public virtual Company Company { get; set; }
    }
}
