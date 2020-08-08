namespace InvestmentManager.Web.Models.PortfolioModels
{
    public class ComissionModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Sum { get; set; }
        public string CurrencyType { get => "rub"; }
    }
}
