using InvestmentManager.Entities.Market;
using System.Collections.Generic;

namespace InvestmentManager.Calculator.Models
{
    public class BuyRecommendationArgs
    {
        public long CompanyId { get; set; }
        public int RatingPlace { get; set; }
        public int CompanyCountWhitPrice { get; set; }
        public IEnumerable<Price> Prices { get; set; }
    }
}
