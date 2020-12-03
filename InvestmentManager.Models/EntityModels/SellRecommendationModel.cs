using System;

namespace InvestmentManager.Models.EntityModels
{
    public class SellRecommendationModel
    {
        public DateTime DateUpdate { get; set; }

        public int LotMin { get; set; }
        public int LotMid { get; set; }
        public int LotMax { get; set; }

        public decimal PriceMin { get; set; }
        public decimal PriceMid { get; set; }
        public decimal PriceMax { get; set; }
    }
}
