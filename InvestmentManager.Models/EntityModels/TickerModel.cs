using InvestmentManager.Models.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class TickerModel : BaseView, IEditebleModel
    {
        [Required,StringLength(10)]
        public override string Name { get; set; }
        public long CompanyId { get; set; }
        public long ExchangeId { get; set; }
        public long LotId { get; set; }
        public bool IsEditeble { get; init; }
    }
}
