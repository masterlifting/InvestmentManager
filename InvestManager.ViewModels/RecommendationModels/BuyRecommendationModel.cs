namespace InvestManager.ViewModels.RecommendationModels
{
    public class BuyRecommendationModel : BaseRecommendationModel
    {
        public decimal BuyPrice { get; set; }
        public bool IsRecommend { get; set; }
    }
}
