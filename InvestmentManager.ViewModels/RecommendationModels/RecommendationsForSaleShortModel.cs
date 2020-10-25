﻿using InvestmentManager.ViewModels.ErrorModels;

namespace InvestmentManager.ViewModels.RecommendationModels
{
    public class RecommendationsForSaleShortModel
    {
        public string DateUpdate { get; set; }

        public int LotMin { get; set; }
        public int LotMid { get; set; }
        public int LotMax { get; set; }

        public string PriceMin { get; set; }
        public string PriceMid { get; set; }
        public string PriceMax { get; set; }

        public ErrorBaseModel Error { get; set; } = new ErrorBaseModel { };
    }
}
