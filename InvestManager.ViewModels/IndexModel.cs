namespace InvestManager.ViewModels
{
    public class IndexModel
    {
        public string CompanyCount { get; set; } = DefaultData.loading;
        public string CompanyDateUpdate { get; set; } = DefaultData.loading;
        public string ReportCount { get; set; } = DefaultData.loading;
        public string ReportDateUpdate { get; set; } = DefaultData.loading;
        public string CoefficientCount { get; set; } = DefaultData.loading;
        public string CoefficientDateUpdate { get; set; } = DefaultData.loading;
        public string PriceCount { get; set; } = DefaultData.loading;
        public string PriceDateUpdate { get; set; } = DefaultData.loading;
        public string BuyRecommendationCount { get; set; } = DefaultData.loading;
        public string BuyRecommendationDateUpdate { get; set; } = DefaultData.loading;
        public string RatingDateUpdate { get; set; } = DefaultData.loading;
    }
}
