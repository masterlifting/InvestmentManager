using System;

namespace InvestmentManager.Models.SummaryModels
{
    public class SummaryBuyRecommendation
    {
        public bool IsHave { get; set; } = false;
        public DateTime DateUpdate { get; set; }
        public decimal BuyPrice { get; set; }
    }
}
