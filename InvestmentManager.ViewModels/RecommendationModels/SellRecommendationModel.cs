namespace InvestmentManager.ViewModels.RecommendationModels
{
    public class SellRecommendationModel : BaseRecommendationModel
    {
        public decimal PriceMin { get; set; }
        public decimal PriceMid { get; set; }
        public decimal PriceMax { get; set; }

        public int ValueMin { get; set; }
        public int ValueMid { get; set; }
        public int ValueMax { get; set; }

        public bool IsRecommendMin { get; set; }
        public bool IsRecommendMid { get; set; }
        public bool IsRecommendMax { get; set; }
    }
}
