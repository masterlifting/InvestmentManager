using InvestmentManager.Entities.Attributes;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class IsinModel : ShortView
    {
        [Required, StringLength(50)]
        public override string Name { get; set; }
        [Zero]
        public long CompanyId { get; set; }
    }
}
