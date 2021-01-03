using InvestmentManager.Entities.Broker;
using System.Collections.Generic;

namespace InvestmentManager.Calculator.Models
{
    public class SellRecommendationArgs
    {
        public int BuyValue { get; set; }
        public int SellValue { get; set; }
        public int Lot { get; set; }
        public int RatingPlace { get; set; }
        public int RatingCount { get; set; }
        public IEnumerable<StockTransaction> StockTransactions { get; set; }
    }
}
