using InvestmentManager.Entities.Attributes;
using InvestmentManager.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models
{
    public class IsinModel : BaseView, IEditebleModel
    {
        [Required, StringLength(50)]
        public override string Name { get; set; }
        [Zero]
        public long CompanyId { get; set; }
        public bool IsEditeble { get; init; }
    }
}
