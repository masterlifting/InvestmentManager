namespace InvestmentManager.Web.Models.CalculateModels
{
    public class SellRecommendationModel
    {
        public string CompanyName { get; set; }
        public int LotMinProfit { get; set; }
        public int LotMidProfit { get; set; }
        public int LotMaxProfit { get; set; }
        public decimal PriceMinProfit { get; set; }
        public decimal PriceMidProfit { get; set; }
        public decimal PriceMaxProfit { get; set; }
        public decimal LastPrice { get; set; }
    }
}
