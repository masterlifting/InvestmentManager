namespace InvestManager.ViewModels.RecommendationModels
{
    public abstract class BaseRecommendationModel
    {
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string LastPriceDate { get; set; }
        public  decimal LastPriceValue { get; set; }
        public long CurrencyId { get; set; }
    }
}
