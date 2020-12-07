using System.ComponentModel.DataAnnotations;

namespace InvestmentManager.Models.EntityModels
{
    public class TickerModel : ShortView
    {
        [Required,StringLength(10)]
        public override string Name { get; set; }
        public long CompanyId { get; set; }
        public long ExchangeId { get; set; }
        public long LotId { get; set; }
    }
}
