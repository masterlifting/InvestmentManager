namespace InvestmentManager.Web.Models.FinancialModels
{
    public class PriceComponentModel
    {
        public long CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal RecommendationPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public string DateUpdate { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string CurrencyType { get; set; }
    }
}
