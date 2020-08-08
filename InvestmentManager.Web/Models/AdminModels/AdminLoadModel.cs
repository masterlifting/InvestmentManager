namespace InvestmentManager.Web.Models.AdminModels
{
    public class AdminLoadModel
    {
        public bool StockPriceFinderInProcess { get; set; }
        public bool ReportFinderInProcess { get; set; }

        public string PriceFinderProcessName { get => "PriceFinder"; }
        public string ReportFinderProcessName { get => "ReportFinder"; }
    }
}
